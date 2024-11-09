using System.Linq.Expressions;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class OrderRepository: GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(DbContext context) : base(context)
    {
    }


    public async Task<IPaginate<Order>> GetOrdersPagingAsyncWithMemberId(int page, int size, Guid memberId,
        IFilter<Order> filter, string sortBy, bool isAsc)
    {
        var orders = await GetPagingListAsync(
            selector: o => new Order()
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                ModifiedAt = o.ModifiedAt,
                Status = o.Status,
                Total = o.Total,
                Address = o.Address,
                MemberId = o.MemberId,
                PaymentId = o.PaymentId,
                Code = o.Code,
                Member = o.Member,
            },
            predicate: o => o.MemberId == memberId,
            page: page,
            size: size,
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            include: o => o.Include(o => o.Member)
                .ThenInclude(m => m.User)
                .Include(o => o.Payment)
        );
        return orders;
        
    }

    public async Task<IPaginate<Order>> GetOrdersPagingAsync(int page, int size, IFilter<Order>? filter, string? sortBy,
        bool isAsc)
    {
        var orders = await GetPagingListAsync(
            selector: o => new Order()
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                ModifiedAt = o.ModifiedAt,
                Status = o.Status,
                Total = o.Total,
                Address = o.Address,
                MemberId = o.MemberId,
                PaymentId = o.PaymentId,
                Code = o.Code,
                Member = o.Member
            },
            page: page,
            size: size,
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            include: o => o.Include(o => o.Member)
                .ThenInclude(m => m.User)
                .Include(o => o.Payment)
        );
        return orders;
    }

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        return await SingleOrDefaultAsync(predicate: o => o.Id == id);
    }

    public async Task<ICollection<Order>> GetOrderList()
    {
        var orders = await GetListAsync(
            include: o => o.Include(o => o.Member)
                .ThenInclude(m => m.User)
                .Include(o => o.Payment)
        );
        return orders;
    }
    public async Task<ICollection<Order>> GetOrdersByDate(DateTime? startDate, DateTime? endDate)
    {
        var orders = await GetListAsync(
            predicate: o => startDate == null || o.CreatedAt >= startDate && (endDate == null || o.CreatedAt <= endDate),
            include: o => o.Include(o => o.Member)
                .ThenInclude(m => m.User)
                .Include(o => o.Payment)
        );
        return orders;
    }
}