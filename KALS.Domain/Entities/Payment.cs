using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;
using KALS.Domain.Enums;

namespace KALS.Domain.Entities;

public class Payment: BaseEntity
{
    public int OrderCode { get; set; } //OrderId of PayOS
    public DateTime? PaymentDateTime { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public PaymentStatus Status { get; set; }
    
    public Order Order { get; set; }
    
}