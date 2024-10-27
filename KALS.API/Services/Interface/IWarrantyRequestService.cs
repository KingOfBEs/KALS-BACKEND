using KALS.API.Models.WarrantyRequest;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface IWarrantyRequestService
{
    Task<IPaginate<WarrantyRequestWithImageResponse>> GetWarrantyRequestsAsync(int page, int size, Guid? memberId);
    Task<WarrantyRequestWithImageResponse> CreateWarrantyRequestAsync(CreateWarrantyRequestRequest request);
    Task<WarrantyRequestWithImageResponse> UpdateWarrantyRequestAsync(Guid warrantyRequestId, UpdateWarrantyRequestRequest request);

}