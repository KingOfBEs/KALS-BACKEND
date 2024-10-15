using KALS.Domain.Entities;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface ISupportRequestRepository: IGenericRepository<SupportRequest>
{
    public Task<ICollection<SupportRequest>> GetSupportRequestIsOpen(Guid memberId);
    
    public Task<SupportRequest> GetSupportRequestById(Guid id);
    public Task<IPaginate<SupportRequest>> GetSupportRequestPagingByMemberIdAsync(Guid memberId, int page, int size, SupportRequestFilter? filter, string? sortBy, bool isAsc);
    public Task<IPaginate<SupportRequest>> GetSupportRequestPagingAsync(int page, int size, SupportRequestFilter? filter, string? sortBy, bool isAsc);
}