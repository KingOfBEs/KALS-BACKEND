using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Order;
using KALS.API.Models.OrderItem;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.API.Services.Implement;

public class OrderService: BaseService<OrderService>, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILabMemberRepository _labMemberRepository;
    public OrderService(ILogger<OrderService> logger, IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IOrderRepository orderRepository, IMemberRepository memberRepository,
        IOrderItemRepository orderItemRepository, IProductRepository productRepository, ILabMemberRepository labMemberRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _orderRepository = orderRepository;
        _memberRepository = memberRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _labMemberRepository = labMemberRepository;
    }

    public async Task<IPaginate<OrderResponse>> GetOrderList(int page, int size, OrderFilter? filter, string? sortBy, bool isAsc)
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        var role = GetRoleFromJwt();
        
        IPaginate<OrderResponse> orderResponses;
        switch (role)
        {
            case RoleEnum.Member:
                var member = await _memberRepository.GetMemberByUserId(userId);
                if(member == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);
                
                var ordersWithMemberId = await _orderRepository.GetOrdersPagingAsyncWithMemberId(page, size, member.Id, filter, sortBy,
                    isAsc);
                orderResponses = _mapper.Map<IPaginate<OrderResponse>>(ordersWithMemberId);
                break;
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                var orders = await _orderRepository.GetOrdersPagingAsync(page, size, filter, sortBy, isAsc);
                orderResponses = _mapper.Map<IPaginate<OrderResponse>>(orders);
                break;
            default:
                throw new BadHttpRequestException(MessageConstant.User.RoleNotFound);
        }
        return orderResponses;
    }

    public async Task<OrderResponse> UpdateOrderStatusCompleted(Guid orderId)
    {
        if (orderId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Order.OrderIdNotNull);
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null) throw new BadHttpRequestException(MessageConstant.Order.OrderNotFound);

        switch (order.Status)
        {
            case OrderStatus.Pending:
                throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsNotPaid);
            case OrderStatus.Cancelled:
                throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsCancelled);
            case OrderStatus.Completed:
                throw new BadHttpRequestException(MessageConstant.Payment.YourOrderIsCompleted);
            case OrderStatus.Processing:
                order.Status = order.Status = OrderStatus.Completed;
                break;
            default:
                throw new BadHttpRequestException(MessageConstant.Order.OrderStatusNotFound);
        }
        
        var orderItems = await _orderItemRepository.GetOrderItemByOrderIdAsync(orderId);
        if(orderItems.Any(oi => oi.Product == null)) 
            throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                foreach (var orderItem in orderItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(orderItem.ProductId);
                    foreach (var lab in product.Labs!)
                    {
                        var existedLabMember = await _labMemberRepository.GetLabMemberByLabIdAndMemberId(lab.Id, order.MemberId);
                        if (existedLabMember != null) continue;
                        await _labMemberRepository.InsertAsync(new LabMember()
                        {
                            MemberId = order.MemberId,
                            LabId = lab.Id
                        });
                    }
                }
                // _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                _orderRepository.UpdateAsync(order);
                // var isOrderSuccess = await _orderRepository.SaveChangesAsync();
                // if (!isOrderSuccess) return null;
                var isInsertLabMemberSuccess = await _labMemberRepository.SaveChangesAsync();
                if (!isInsertLabMemberSuccess) return null;
                transaction.Complete();
                OrderResponse response = _mapper.Map<OrderResponse>(order);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
    public async Task<ICollection<OrderItemResponse>> GetOrderItemsByOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Order.OrderIdNotNull);
        
        var orderItems = await _orderItemRepository.GetOrderItemByOrderIdAsync(orderId);
        var response = _mapper.Map<ICollection<OrderItemResponse>>(orderItems);
        return response;
    } 
}