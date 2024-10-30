using System.Linq.Expressions;
using KALS.Domain.Entities;
using KALS.Domain.Enums;

namespace KALS.Domain.Filter.FilterModel;

public class WarrantyRequestFilter: IFilter<WarrantyRequest>
{
    public string? Name { get; set; }
    public WarrantyRequestStatus? Status { get; set; }
    public Expression<Func<WarrantyRequest, bool>> ToExpression()
    {
        return warrantyRequest =>
            (string.IsNullOrEmpty(Name) || warrantyRequest.OrderItem.Order.Member.User.FullName.Contains(Name)) &&
            (!Status.HasValue || warrantyRequest.Status == Status);
    }
}