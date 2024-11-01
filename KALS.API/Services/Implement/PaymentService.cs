using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Models.Payment;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json;
using StackExchange.Redis;
using Order = KALS.Domain.Entities.Order;

namespace KALS.API.Services.Implement;

public class PaymentService: BaseService<PaymentService>, IPaymentService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IRedisService _redisService;
    public PaymentService(ILogger<PaymentService> logger, IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IMemberRepository memberRepository, IPaymentRepository paymentRepository, IProductRepository productRepository,
        IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IRedisService redisService) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _memberRepository = memberRepository;
        _paymentRepository = paymentRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _redisService = redisService;
    }

    public async Task<string> CheckOut(CheckOutRequest request)
    {
        PayOS payOs = new PayOS(_configuration["PAYOS:PAYOS_CLIENT_ID"]!,
            _configuration["PAYOS:PAYOS_API_KEY"]!,
            _configuration["PAYOS:PAYOS_CHECKSUM_KEY"]!);
        
        var userId = GetUserIdFromJwt();
        
        var member = await _memberRepository.GetMemberByUserId(userId);
        if (member == null) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        if( member.Commune == null || member.Province == null || member.District == null || member.Address == null) 
            throw new BadHttpRequestException(MessageConstant.User.MemberAddressNotFound);
        
        var key = "Cart:" + userId;
        var cartData = await _redisService.GetStringAsync(key);

        if (string.IsNullOrEmpty(cartData)) throw new BadHttpRequestException(MessageConstant.Cart.CartNotFound);
        
        var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        
        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        var order = new Order()
        {
            Id = Guid.NewGuid(),
            Code = "ORDER-" + orderCode,
            CreatedAt = TimeUtil.GetCurrentSEATime(),
            ModifiedAt = TimeUtil.GetCurrentSEATime(),
            Status = OrderStatus.Pending,
            MemberId = member.Id
        };
        
        var orderItems = new List<OrderItem>();
        decimal orderTotal = 0;
        List<ItemData> items = new List<ItemData>();
        
        foreach (var cartModel in cart)
        {
            
            var product = await _productRepository.GetProductByIdAsync(cartModel.ProductId);
            if (product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
            if (product.Quantity < cartModel.Quantity) throw new BadHttpRequestException(MessageConstant.Product.ProductOutOfStock);
            var orderItem = new OrderItem()
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = cartModel.Quantity,
                CreatedAt = TimeUtil.GetCurrentSEATime(),
                ModifiedAt =TimeUtil.GetCurrentSEATime(),
                Order = order,
                WarrantyCode = CodeUtil.GenerateWarrantyCode(product.Id),
                WarrantyExpired = null
            };
            orderItems.Add(orderItem);
            
            decimal itemTotal = product.Price * cartModel.Quantity;
            orderTotal += itemTotal;
            var item = new ItemData(product.Name, cartModel.Quantity, (int) itemTotal);
            items.Add(item);
        }

        order.Total = orderTotal;
        order.Address = request.Address;
        var payment = new Payment()
        {
            Id = Guid.NewGuid(),
            OrderCode = orderCode,
            CreatedAt = TimeUtil.GetCurrentSEATime(),
            ModifiedAt = TimeUtil.GetCurrentSEATime(),
            Status = PaymentStatus.Processing,
            Amount = order.Total,
            Order = order
        };
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                await _orderRepository.InsertAsync(order);
                await _paymentRepository.InsertAsync(payment);
                
                foreach (var orderItem in orderItems) 
                { 
                    orderItem.OrderId = order.Id; 
                } 
                await _orderItemRepository.InsertRangeAsync(orderItems);
                
                var isSuccess = await _orderRepository.SaveChangesAsync(); 
                transaction.Complete(); 
                if (!isSuccess) throw new BadHttpRequestException(MessageConstant.Order.CreateOrderFail);
                PaymentData paymentData = new PaymentData(
                    orderCode, 
                    (int)order.Total, 
                    "Thanh toán đơn hàng", 
                    items, 
                    "https://stemlabs.store/cancel", 
                    "https://stemlabs.store/success",
                    buyerName: member.User.FullName, 
                    buyerPhone: member.User.PhoneNumber,
                    expiredAt: ((DateTimeOffset) TimeUtil.GetCurrentSEATime().AddMinutes(10)).ToUnixTimeSeconds()
                    );
                // Call the external payment service to create a payment link
                CreatePaymentResult createPayment = await payOs.createPaymentLink(paymentData);
                
                return createPayment.checkoutUrl;
            }
            catch (TransactionException ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
    
    public async Task<PaymentWithOrderResponse> HandlePayment(UpdatePaymentOrderStatusRequest request)
    {
        if(request.OrderCode == 0) throw new BadHttpRequestException(MessageConstant.Payment.OrderCodeNotNull);
        
        var payment = await _paymentRepository.GetPaymentByOrderCode(request.OrderCode);
        if (payment == null) throw new BadHttpRequestException(MessageConstant.Payment.PaymentNotFound);
        
        if (payment.Status == PaymentStatus.Paid)
            throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsPaid);
        if (payment.Status == PaymentStatus.Fail)
            throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsCancelled);
        
        PayOS _payOs = new PayOS(_configuration["PAYOS:PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find environment"),
            _configuration["PAYOS:PAYOS_API_KEY"] ?? throw new Exception("Cannot find environment"),
            _configuration["PAYOS:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find environment"));
        
        PaymentLinkInformation paymentLinkInformation = await _payOs.getPaymentLinkInformation(request.OrderCode);
        if(paymentLinkInformation == null) 
            throw new BadHttpRequestException(MessageConstant.Payment.CannotFindPaymentLinkInformation);
        if (paymentLinkInformation.status == PayOsStatus.PENDING.ToString())
            throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsNotPaid);
        
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                switch (EnumUtil.ParseEnum<PayOsStatus>(paymentLinkInformation.status))
                {
                    case PayOsStatus.PAID:
                        payment.Status = PaymentStatus.Paid;
                        payment.ModifiedAt = TimeUtil.GetCurrentSEATime();
                        payment.PaymentDateTime =
                            DateTime.Parse(paymentLinkInformation.transactions[0].transactionDateTime);
                        payment.Order.Status = OrderStatus.Processing;
                        payment.Order.ModifiedAt = TimeUtil.GetCurrentSEATime();
                        _paymentRepository.UpdateAsync(payment);

                        var orderItems = await _orderItemRepository.GetOrderItemByOrderIdAsync(payment.Order.Id);
                        foreach (var orderItem in orderItems)
                        {
                            orderItem.Product.Quantity -= orderItem.Quantity;
                        }

                        _orderItemRepository.UpdateRangeAsync(orderItems);
                        break;
                    case PayOsStatus.EXPIRED:
                    case PayOsStatus.CANCELLED:
                        payment.Status = PaymentStatus.Fail;
                        payment.ModifiedAt = TimeUtil.GetCurrentSEATime();
                        payment.Order.Status = OrderStatus.Cancelled;
                        payment.Order.ModifiedAt = TimeUtil.GetCurrentSEATime();
                        // _unitOfWork.GetRepository<Payment>().UpdateAsync(payment);
                        _paymentRepository.UpdateAsync(payment);
                        break;
                    default:
                        throw new BadHttpRequestException(MessageConstant.Payment.PayOsStatusNotTrue);
                }

                bool isSuccess = await _paymentRepository.SaveChangesAsync();
                transaction.Complete();
                PaymentWithOrderResponse response = null;
                if (isSuccess) response = _mapper.Map<PaymentWithOrderResponse>(payment);
                return response;
            }
            catch (TransactionException ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }

    public async Task<bool> UpdateExpiredPayment()
    {
        
        var paymentExpires = await _paymentRepository.GetPaymentExpiredList();
        var hasExpiredPayments = false;
        if (paymentExpires.Any())
        {
            foreach (var paymentExpire in paymentExpires)
            {
                
                paymentExpire.Status = PaymentStatus.Fail;
                paymentExpire.ModifiedAt = TimeUtil.GetCurrentSEATime();
                paymentExpire.Order.Status = OrderStatus.Cancelled;
                paymentExpire.Order.ModifiedAt = TimeUtil.GetCurrentSEATime();
                _paymentRepository.UpdateAsync(paymentExpire);
                hasExpiredPayments = true;
                Console.Write("Exist payment has expired");
            }
        }
        if (hasExpiredPayments)
        {
            var isSuccess = await _paymentRepository.SaveChangesAsync();
            return isSuccess;
        }
        return false;
    }
}