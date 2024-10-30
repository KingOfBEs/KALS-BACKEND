using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KALS.Repository.Implement;

public class LabRepository: GenericRepository<Lab>, ILabRepository
{
    public LabRepository(DbContext context) : base(context)
    {
    }

    public async Task<Lab> GetLabByIdAsync(Guid id, Guid? memberId)
    {
        var lab = await SingleOrDefaultAsync(
            predicate: l => l.Id == id,
            include: l => l.Include(l =>  l.LabMembers!.Where(lm => memberId == null || lm.MemberId == memberId))
                .ThenInclude(lm => lm.Member)
                .Include(l => l.Product)
                .ThenInclude(p => p.ProductImages)
        );
        return lab;
    }

    public async Task<Lab> GetLabByIdNoProductAsync(Guid id)
    {
        var lab = await SingleOrDefaultAsync(
            predicate: l => l.Id == id
        );
        return lab;
    }

    public async Task<IPaginate<Lab>> GetLabsPagingByMemberId(Guid memberId, int page, int size, string? searchName)
    {
        var labs = await GetPagingListAsync(
            selector: l => new Lab()
            {
                Id = l.Id,
                Name = l.Name,
                CreatedAt = l.CreatedAt,
                ModifiedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                ModifiedBy = l.ModifiedBy,
                Url = l.Url,
                LabMembers = l.LabMembers,
                ProductId = l.ProductId,
                Product = l.Product,
            },
            predicate: l => l.LabMembers!.Any(lm => lm.MemberId.Equals(memberId)) && 
                            (searchName.IsNullOrEmpty() || l.Name.Contains(searchName!)),
            page: page,
            size: size,
            orderBy: l => l.OrderByDescending(l => l.CreatedAt),
            filter: null,
            include: l => l.Include(l => l.LabMembers!.Where(lm => lm.MemberId.Equals(memberId)))
                .ThenInclude(lm => lm.Member)
                .Include(l => l.Product)
                .ThenInclude(p => p.ProductImages)
        );
        return labs;
    }

    public async Task<IPaginate<Lab>> GetLabsPagingAsync(int page, int size, string? searchName)
    {
        var labs = await GetPagingListAsync(
            selector: l => new Lab()
            {
                Id = l.Id,
                Name = l.Name,
                CreatedAt = l.CreatedAt,
                ModifiedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                ModifiedBy = l.ModifiedBy,
                Url = l.Url,
                Product = l.Product
            },
            predicate: l => (searchName.IsNullOrEmpty() || l.Name.Contains(searchName!)),
            page: page,
            size: size,
            orderBy: l => l.OrderByDescending(l => l.CreatedAt),
            filter: null,
            include: l => l.Include(l => l.Product)
                .ThenInclude(p => p.ProductImages)
            
        );
        return labs;
    }
    
}