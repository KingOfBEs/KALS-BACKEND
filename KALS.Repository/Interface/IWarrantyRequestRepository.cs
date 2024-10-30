using KALS.Domain.Entities;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IWarrantyRequestRepository: IGenericRepository<WarrantyRequest>
{
    Task<IPaginate<WarrantyRequest>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId, WarrantyRequestFilter? filter, string? sortBy, bool isAsc);
    Task<WarrantyRequest> GetWarrantyRequestByIdAsync(Guid warrantyRequestId);
}