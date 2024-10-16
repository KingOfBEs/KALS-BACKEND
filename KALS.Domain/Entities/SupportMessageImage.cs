using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;

namespace KALS.Domain.Entities;

public class SupportMessageImage: BaseEntity
{
    public string ImageUrl { get; set; }
    public Guid SupportMessageId { get; set; }
    [ForeignKey(nameof(SupportMessageId))]
    public SupportMessage SupportMessage { get; set; }
}