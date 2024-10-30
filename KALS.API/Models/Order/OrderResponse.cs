using KALS.API.Models.User;
using KALS.Domain.Enums;

namespace KALS.API.Models.Order;

public class OrderResponse
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    public MemberResponse Member { get; set; }
}