using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class OrderItemRepository: GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(DbContext context) : base(context)
    {
    }

    public Task<ICollection<OrderItem>> GetOrderItemByOrderIdAsync(Guid orderId)
    {
        var orderItem = GetListAsync(
            predicate: oi => oi.OrderId == orderId,
            include: oi => oi.Include(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
        );
        return orderItem;
    }

    public async Task<OrderItem> GetOrderItemByIdAsync(Guid orderItemId)
    {
        var orderItem = await SingleOrDefaultAsync(
            predicate: oi => oi.Id == orderItemId,
            include: oi => oi.Include(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(oi => oi.Order)
                .ThenInclude(o => o.Member)
        );
        return orderItem;
    }
}