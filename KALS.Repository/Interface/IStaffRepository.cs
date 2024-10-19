using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IStaffRepository: IGenericRepository<Staff>
{
    Task<IPaginate<Staff>> GetStaffPagingAsync(int page, int size, IFilter<Staff> filter, string sortBy, bool isAsc);
    
    Task<Staff> GetStaffByUserIdAsync(Guid userId);
    
    Task<Staff> GetStaffByIdAsync(Guid id);
}