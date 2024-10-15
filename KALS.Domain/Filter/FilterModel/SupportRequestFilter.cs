using System.Linq.Expressions;
using KALS.Domain.Entities;
using KALS.Domain.Enums;

namespace KALS.Domain.Filter.FilterModel;

public class SupportRequestFilter: IFilter<SupportRequest>
{
    public DateTime? CreatedAt { get; set; }
    public SupportRequestStatus? Status { get; set; }
    public Expression<Func<SupportRequest, bool>> ToExpression()
    {
        return supportRequest =>
            (!CreatedAt.HasValue || supportRequest.CreatedAt == CreatedAt) &&
            (!Status.HasValue || supportRequest.Status == Status);
    }
}