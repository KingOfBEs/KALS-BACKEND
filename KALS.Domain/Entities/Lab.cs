using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;

namespace KALS.Domain.Entities;

public class Lab: BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(500)]
    public string Url { get; set; }
    [MaxLength(1000)]
    public string VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
    
    public Guid ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    public virtual ICollection<LabMember>? LabMembers { get; set; }
}