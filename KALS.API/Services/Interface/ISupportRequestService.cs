using KALS.API.Models.SupportRequest;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface ISupportRequestService
{
    Task<SupportRequestResponse> CreateSupportRequest(SupportRequest request);
    
    Task<SupportRequestResponse> ResponseSupportMessage( Guid supportRequestId, ResponseSupportRequest request);
    
    Task<IPaginate<SupportRequestResponse>> GetSupportRequestPagingAsync(int page, int size, SupportRequestFilter? filter, string? sortBy, bool isAsc);
    
    Task<SupportRequestResponse> GetSupportRequestByIdAsync(Guid id);
}