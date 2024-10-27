using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IWarrantyRequestRepository: IGenericRepository<WarrantyRequest>
{
    Task<IPaginate<WarrantyRequest>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId);
    Task<WarrantyRequest> GetWarrantyRequestByIdAsync(Guid warrantyRequestId);
}