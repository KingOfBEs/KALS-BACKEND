using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class LabMemberRepository: GenericRepository<LabMember>, ILabMemberRepository
{
    public LabMemberRepository(DbContext context) : base(context)
    {
    }

    public async Task<LabMember> GetLabMemberByLabIdAndMemberId(Guid labId, Guid memberId)
    {
        var labMember = await SingleOrDefaultAsync(
            predicate: lm => lm.LabId == labId && lm.MemberId == memberId,
            include: lm => lm.Include(lm => lm.Lab)
                .Include(lm => lm.Member)
        );
        return labMember;
    }

    public async Task<LabMember> GetLabMemberByLabIdAndMemberIdNoInclude(Guid labId, Guid memberId)
    {
        return await SingleOrDefaultAsync(
            predicate: lm => lm.LabId == labId && lm.MemberId == memberId
        );
    }

    public async Task<ICollection<LabMember>> GetLabMembersByLabIds(List<Guid> labIds)
    {
        var labMembers = await GetListAsync(
            predicate: lm => labIds.Contains(lm.LabId)
        );
        return labMembers;
    }

    public async Task<bool> IsMemberInLab(Guid memberId, Guid labId)
    {
        var labMember = await SingleOrDefaultAsync(
            predicate: lm => lm.LabId == labId && lm.MemberId == memberId
        );
        return labMember != null;
    }
}