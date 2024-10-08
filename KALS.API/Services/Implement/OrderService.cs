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

        RoleEnum roleEnum = EnumUtil.ParseEnum<RoleEnum>(role);
        IPaginate<OrderResponse> orderResponses;
        switch (roleEnum)
        {
            case RoleEnum.Member:
                // var member = await _unitOfWork.GetRepository<Member>().SingleOrDefaultAsync(
                //     predicate: x => x.UserId == userId
                // );
                var member = await _memberRepository.GetMemberByUserId(userId);
                if(member == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);
                //  orders = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                //     selector: o => new OrderResponse()
                //     {
                //         Id = o.Id,
                //         CreatedAt = o.CreatedAt,
                //         ModifiedAt = o.ModifiedAt,
                //         Status = o.Status,
                //         Total = o.Total,
                //         Address = o.Address,
                //     },
                //     predicate: o => o.MemberId == member.Id,
                //     page: page,
                //     size: size,
                //     filter: filter,
                //     sortBy: sortBy,
                //     isAsc: isAsc
                // );
                var ordersWithMemberId = await _orderRepository.GetOrdersPagingAsyncWithMemberId(page, size, member.Id, filter, sortBy,
                    isAsc);
                orderResponses = _mapper.Map<IPaginate<OrderResponse>>(ordersWithMemberId);
                break;
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                // orders = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                //     selector: o => new OrderResponse()
                //     {
                //         Id = o.Id,
                //         CreatedAt = o.CreatedAt,
                //         ModifiedAt = o.ModifiedAt,
                //         Status = o.Status,
                //         Total = o.Total,
                //         Address = o.Address,
                //     },
                //     page: page,
                //     size: size,
                //     filter: filter,
                //     sortBy: sortBy,
                //     isAsc: isAsc
                // );
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
        // var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
        //     predicate: o => o.Id == orderId
        // );
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
        
        // var orderItems = await _unitOfWork.GetRepository<OrderItem>().GetListAsync(
        //     predicate: oi => oi.OrderId == orderId
        var orderItems = await _orderItemRepository.GetOrderItemByOrderIdAsync(orderId);
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                foreach (var orderItem in orderItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(orderItem.ProductId);
                    if (product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
                    foreach (var labProduct in product.LabProducts)
                    {
                        var existedLabMember = _labMemberRepository.GetLabMemberByLabIdAndMemberId(labProduct.LabId, order.MemberId);
                        if (existedLabMember != null) continue;
                        await _labMemberRepository.InsertAsync(new LabMember()
                        {
                            MemberId = order.MemberId,
                            LabId = labProduct.LabId
                        });
                        var isInsertLabMemberSuccess = await _labMemberRepository.SaveChangesAsync();
                        if (!isInsertLabMemberSuccess) return null;
                    }
                }
                // _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                _orderRepository.UpdateAsync(order);
                var isOrderSuccess = await _orderRepository.SaveChangesWithTransactionAsync();
                if (!isOrderSuccess) return null;
                transaction.Complete();
                OrderResponse response = _mapper.Map<OrderResponse>(order);
                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
    }

    public async Task<ICollection<OrderItemResponse>> GetOrderItemsByOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Order.OrderIdNotNull);
        // var orderItems = await _unitOfWork.GetRepository<OrderItem>().GetListAsync(
        //     predicate: oi => oi.OrderId == orderId,
        //     include: oi => oi.Include(oi => oi.Product)
        // );
        var orderItems = await _orderItemRepository.GetOrderItemByOrderIdAsync(orderId);
        var response = _mapper.Map<ICollection<OrderItemResponse>>(orderItems);
        return response;
    } 
}