using System.Linq.Expressions;
using KALS.Domain.Entities;
using KALS.Domain.Enums;

namespace KALS.Domain.Filter.FilterModel;

public class OrderFilter: IFilter<Order>
{
    public OrderStatus? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Username { get; set; }
    
    public Expression<Func<Domain.Entities.Order, bool>> ToExpression()
    {
        return order => 
            (!Status.HasValue || order.Status == Status) &&
            (!CreatedAt.HasValue || order.CreatedAt == CreatedAt) &&
            (string.IsNullOrEmpty(Username) || order.Member.User.Username.Contains(Username));
    }
}