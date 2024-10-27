using System.ComponentModel.DataAnnotations;
using KALS.Domain.Common;

namespace KALS.Domain.Entities;

public class Lab: BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(500)]
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
    public virtual ICollection<LabProduct>? LabProducts { get; set; }
    public virtual ICollection<LabMember>? LabMembers { get; set; }
}