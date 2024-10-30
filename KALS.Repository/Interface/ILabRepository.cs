using KALS.Domain.Entities;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface ILabRepository: IGenericRepository<Lab>
{
    Task<Lab> GetLabByIdAsync(Guid id, Guid? memberId);
    
    Task<Lab> GetLabByIdNoProductAsync(Guid id);
    
    Task<IPaginate<Lab>> GetLabsPagingByMemberId(Guid memberId, int page, int size, string? searchName);
    
    Task<IPaginate<Lab>> GetLabsPagingAsync(int page, int size, string? searchName);
    
}