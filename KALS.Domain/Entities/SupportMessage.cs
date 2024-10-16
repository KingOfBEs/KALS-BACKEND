using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;
using KALS.Domain.Enums;

namespace KALS.Domain.Entities;

public class SupportMessage: BaseEntity
{
    public SupportMessageType Type { get; set; }
    [MaxLength(Int32.MaxValue)]
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid SupportRequestId { get; set; }
    [ForeignKey(nameof(SupportRequestId))]
    public SupportRequest SupportRequest { get; set; }
    
    public virtual ICollection<SupportMessageImage> SupportMessageImages { get; set; }
}