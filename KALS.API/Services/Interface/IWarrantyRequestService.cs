using KALS.API.Models.WarrantyRequest;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface IWarrantyRequestService
{
    Task<IPaginate<WarrantyRequestWithImageResponse>> GetWarrantyRequestsAsync(int page, int size, WarrantyRequestFilter? filter, string? sortBy, bool isAsc);
    Task<WarrantyRequestWithImageResponse> CreateWarrantyRequestAsync(CreateWarrantyRequestRequest request);
    Task<WarrantyRequestWithImageResponse> UpdateWarrantyRequestAsync(Guid warrantyRequestId, UpdateWarrantyRequestRequest request);
    Task<WarrantyRequestWithImageResponse> GetWarrantyRequestByIdAsync(Guid warrantyRequestId);
}