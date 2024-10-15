using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;
using KALS.Domain.Enums;

namespace KALS.Domain.Entities;

public class Order: BaseEntity
{
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public string Code { get; set; }
    [MaxLength(1000)]
    public string Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid MemberId { get; set; }
    [ForeignKey(nameof(MemberId))]
    public Member Member { get; set; }
    
    public Guid PaymentId { get; set; }
    [ForeignKey(nameof(PaymentId))]
    public Payment Payment { get; set; }
    
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}