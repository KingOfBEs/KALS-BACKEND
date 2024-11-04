using KALS.API.Models.Order;
using KALS.API.Models.OrderItem;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface IOrderService
{
    Task<IPaginate<OrderResponse>> GetOrderList(int page, int size, OrderFilter? filter, string? sortBy, bool isAsc);
    
    Task<OrderResponse> UpdateOrderStatusCompleted(Guid orderId);
    
    Task<ICollection<OrderItemResponse>> GetOrderItemsByOrderId(Guid orderId);
    
    Task<OrderResponse> UpdateOrderStatusShipping(Guid orderId);
    
    Task<OrderResponse> UpdateOrderStatusCancelled(Guid orderId);
}