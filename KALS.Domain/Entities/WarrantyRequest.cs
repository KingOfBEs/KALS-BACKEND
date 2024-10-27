using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;
using KALS.Domain.Enums;

namespace KALS.Domain.Entities;

public class WarrantyRequest: BaseEntity
{
    [MaxLength(1000)]
    public string RequestContent { get; set; }
    [MaxLength(1000)]
    public string? ResponseContent { get; set; }
    public WarrantyRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid? ResponseBy { get; set; }
    public Guid OrderItemId { get; set; }
    [ForeignKey(nameof(OrderItemId))]
    public OrderItem OrderItem { get; set; }
    
    public ICollection<WarrantyRequestImage> WarrantyRequestImages { get; set; }
}