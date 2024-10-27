using KALS.Domain.Entities;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class WarrantyRequestRepository: GenericRepository<WarrantyRequest>, IWarrantyRequestRepository
{
    public WarrantyRequestRepository(DbContext context) : base(context)
    {
    }

    public async Task<IPaginate<WarrantyRequest>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId)
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
            },
            predicate: memberId == null ? null : wr => wr.OrderItem.Order.MemberId == memberId,
            page: page,
            size: size,
            filter: null
        );
        return warrantyRequests;
    }

    public async Task<WarrantyRequest> GetWarrantyRequestByIdAsync(Guid warrantyRequestId)
    {
        var warrantyRequest = await SingleOrDefaultAsync(
            predicate: wr => wr.Id == warrantyRequestId
        );
        return warrantyRequest;
    }
}