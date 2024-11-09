using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface ILabMemberRepository: IGenericRepository<LabMember>
{
    Task<LabMember> GetLabMemberByLabIdAndMemberId(Guid labId, Guid memberId);
    
    Task<LabMember> GetLabMemberByLabIdAndMemberIdNoInclude(Guid labId, Guid memberId);
    Task<ICollection<LabMember>> GetLabMembersByLabIds(List<Guid> labIds);
    Task<bool> IsMemberInLab(Guid memberId, Guid labId);
}