using System.Collections;
using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface IOrderItemRepository: IGenericRepository<OrderItem>
{
    Task<ICollection<OrderItem>> GetOrderItemByOrderIdAsync(Guid orderId);
    
    Task<ICollection<OrderItem>> GetOrderItemByOrderIdNoInclude(Guid orderId);
    Task<OrderItem> GetOrderItemByIdAsync(Guid orderItemId);
}