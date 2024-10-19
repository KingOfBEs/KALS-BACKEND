using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class LabProductRepository: GenericRepository<LabProduct>, ILabProductRepository
{
    public LabProductRepository(DbContext context) : base(context)
    {
    }

    public async Task<(List<Guid> newLabIds, List<Guid> removeLabIds)> GetNewAndRemoveLabIdsAsync(Guid productId, List<Guid> requestedLabIds)
    {
        var labProducts = await GetListAsync(
            predicate: lp => lp.ProductId == productId
        );
        var labProductIds = labProducts.Select(lp => lp.LabId).ToList();
        var newLabIds = requestedLabIds.Except(labProductIds).ToList();
        var removeLabIds = labProductIds.Except(requestedLabIds).ToList();
        return (newLabIds, removeLabIds);
    }

    public async Task<ICollection<LabProduct>> GetLabProductsByLabIds(List<Guid> labIds)
    {
        var labProducts = await GetListAsync(
            predicate: lp => labIds.Contains(lp.LabId)
        );
        return labProducts;
    }
}