using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class StaffRepository: GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(DbContext context) : base(context)
    {
        
    }

    public async Task<IPaginate<Staff>> GetStaffPagingAsync(int page, int size, IFilter<Staff> filter, string sortBy,
        bool isAsc)
    {
        var staffs = await GetPagingListAsync(
            selector: s => new Staff()
            {
                Id = s.Id,
                Type = s.Type,
                User = s.User,
                UserId = s.UserId
            },
            page: page,
            size: size,
            filter: filter,
            include: s => s.Include(s => s.User),
            sortBy: sortBy,
            isAsc: isAsc
        );
        return staffs;
    }

    public async Task<Staff> GetStaffByUserIdAsync(Guid userId)
    {
        var staff = await SingleOrDefaultAsync(
            predicate: s => s.UserId == userId,
            include: s => s.Include(s => s.User)
        );
        return staff;
    }

    public async Task<Staff> GetStaffByIdAsync(Guid id)
    {
        var staff = await SingleOrDefaultAsync(
            predicate: s => s.Id == id,
            include: s => s.Include(s => s.User)
        );
        return staff;
    }
}