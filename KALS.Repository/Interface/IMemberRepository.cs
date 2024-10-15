using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IMemberRepository: IGenericRepository<Member>
{
    Task<Member> GetMemberByUserId(Guid userId);
    
    Task<IPaginate<Member>> GetMembersPagingAsync(int page, int size, IFilter<Member> filter, string? sortBy,
        bool isAsc);
    
    Task<Member> GetMemberById(Guid id);
    
    Task<ICollection<Member>> GetMembersOrderProductAsync(Guid productId);
}