using KALS.Domain.Entities;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class WarrantyRequestRepository: GenericRepository<WarrantyRequest>, IWarrantyRequestRepository
{
    public WarrantyRequestRepository(DbContext context) : base(context)
    {
    }

    public async Task<IPaginate<WarrantyRequest>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId, WarrantyRequestFilter? filter, string? sortBy, bool isAsc)
    {
        var warrantyRequests = await GetPagingListAsync(
            selector: wr => new WarrantyRequest()
            {
                Id = wr.Id,
                RequestContent = wr.RequestContent,
                ResponseContent = wr.ResponseContent,
                CreatedAt = wr.CreatedAt,
                ModifiedAt = wr.ModifiedAt,
                Status = wr.Status,
                ResponseBy = wr.ResponseBy,
                OrderItemId = wr.OrderItemId,
                OrderItem = wr.OrderItem,
                WarrantyRequestImages = wr.WarrantyRequestImages
            },
            predicate: memberId == null ? null : wr => wr.OrderItem.Order.MemberId == memberId,
            page: page,
            size: size,
            filter: filter,
            include: wr => wr.Include(wr => wr.OrderItem)
                .ThenInclude(wr => wr.Order)
                .ThenInclude(wr => wr.Member)
                .ThenInclude(wr => wr.User)
                .Include(wr => wr.WarrantyRequestImages),
            sortBy: sortBy,
            isAsc: isAsc
        );
        return warrantyRequests;
    }

    public async Task<WarrantyRequest> GetWarrantyRequestByIdAsync(Guid warrantyRequestId)
    {
        var warrantyRequest = await SingleOrDefaultAsync(
            predicate: wr => wr.Id == warrantyRequestId,
            include: sr => sr.Include(wr => wr.OrderItem)
                .ThenInclude(wr => wr.Order)
                .ThenInclude(wr => wr.Member)
                .ThenInclude(wr => wr.User)
                .Include(wr => wr.WarrantyRequestImages)
        );
        return warrantyRequest;
    }

    public async Task<WarrantyRequest> GetWarrantyRequestByOrderItemId(Guid orderItemId)
    {
        var warrantyRequest = await SingleOrDefaultAsync(
            predicate:wr => wr.OrderItemId == orderItemId
        );
        return warrantyRequest;
    }
}