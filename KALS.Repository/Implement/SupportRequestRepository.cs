using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class SupportRequestRepository: GenericRepository<SupportRequest>, ISupportRequestRepository
{
    public SupportRequestRepository(DbContext context) : base(context)
    {
    }

    public async Task<ICollection<SupportRequest>> GetSupportRequestIsOpen(Guid memberId)
    {
        var supportRequests = await GetListAsync(
            predicate: sr => sr.MemberId == memberId && sr.Status == SupportRequestStatus.Open
        );
        return supportRequests;
    }

    public async Task<SupportRequest> GetSupportRequestById(Guid id)
    {
        var supportRequest = await SingleOrDefaultAsync(
            predicate: sr => sr.Id == id
        );
        return supportRequest;
    }

    public Task<IPaginate<SupportRequest>> GetSupportRequestPagingByMemberIdAsync(Guid memberId, int page, int size, SupportRequestFilter? filter,
        string? sortBy, bool isAsc)
    {
        var supportRequest = GetPagingListAsync(
            selector: sr => new SupportRequest()
            {
                Id = sr.Id,
                MemberId = sr.MemberId,
                Member = sr.Member,
                Lab = sr.Lab,
                Staff = sr.Staff,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                LabId = sr.LabId,
                LabMember = sr.LabMember,
                ModifiedAt = sr.ModifiedAt,
                StaffId = sr.StaffId,
                SupportMessages = sr.SupportMessages
            },
            predicate: sr => sr.MemberId == memberId,
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            page: page,
            size: size,
            include: sr => sr.Include(sr => sr.Member)
                .ThenInclude(m => m.User)
                .Include(sr => sr.Lab)
                .Include(sr => sr.Staff)
                .Include(sr => sr.LabMember)
                .ThenInclude(sr => sr.Lab)
                .Include(sr => sr.SupportMessages)
                .ThenInclude(sm => sm.SupportMessageImages)
        );
        return supportRequest;
    }

    public async Task<IPaginate<SupportRequest>> GetSupportRequestPagingAsync(int page, int size, SupportRequestFilter? filter, string? sortBy, bool isAsc)
    {
        var supportRequest = await GetPagingListAsync(
            selector: sr => new SupportRequest()
            {
                Id = sr.Id,
                MemberId = sr.MemberId,
                Member = sr.Member,
                Lab = sr.Lab,
                Staff = sr.Staff,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                LabId = sr.LabId,
                LabMember = sr.LabMember, 
                ModifiedAt = sr.ModifiedAt,
                StaffId = sr.StaffId,
                SupportMessages = sr.SupportMessages
            },
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc,
            page: page,
            size: size,
            include: sr => sr.Include(sr => sr.Member)
                .ThenInclude(m => m.User)
                .Include(sr => sr.Lab)
                .Include(sr => sr.Staff)
                .Include(sr => sr.LabMember)
                .ThenInclude(lm => lm.Lab)
                .Include(sr => sr.SupportMessages)
                .ThenInclude(sm => sm.SupportMessageImages)
        );
        return supportRequest;
    }
}