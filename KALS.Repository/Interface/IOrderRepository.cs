using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IOrderRepository: IGenericRepository<Order>
{
    Task<IPaginate<Order>> GetOrdersPagingAsyncWithMemberId(int page, int size, Guid memberId, IFilter<Order> filter,
        string sortBy, bool isAsc);
    
    Task<IPaginate<Order>> GetOrdersPagingAsync(int page, int size, IFilter<Order>? filter, string? sortBy, bool isAsc);
    
    Task<Order> GetOrderByIdAsync(Guid id);

    Task<ICollection<Order>> GetOrderList();
    
    Task<ICollection<Order>> GetOrdersByDate(DateTime? startDate, DateTime? endDate);
}