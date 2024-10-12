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
    public OrderRepository(KitAndLabDbContext context) : base(context)
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
                Code = o.Code
            },
            predicate: o => o.MemberId == memberId,
            page: page,
            size: size,
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            include: o => o.Include(o => o.Member)
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
                Code = o.Code
            },
            page: page,
            size: size,
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            include: o => o.Include(o => o.Member)
                .Include(o => o.Payment)
        );
        return orders;
    }

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        return await SingleOrDefaultAsync(predicate: o => o.Id == id);
    }
}