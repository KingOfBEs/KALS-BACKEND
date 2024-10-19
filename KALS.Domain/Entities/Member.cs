using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;

namespace KALS.Domain.Entities;

public class Member : BaseEntity
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    [MaxLength(255)]
    public string? Province { get; set; }
    public string? ProvinceCode { get; set; }
    [MaxLength(255)]
    public string? District { get; set; }
    public string? DistrictCode { get; set; }
    [MaxLength(255)]
    public string? Commune { get; set; }
    public string? CommuneCode { get; set; }
    [MaxLength(500)]
    public string? Address { get; set; }
    
    
    public virtual ICollection<LabMember>? LabMembers { get; set; }
    
    public virtual ICollection<Order>? Orders { get; set; }
}