using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class ProductRelationshipRepository: GenericRepository<ProductRelationship>, IProductRelationshipRepository
{
    public ProductRelationshipRepository(DbContext context) : base(context)
    {
    }

    public async Task<(List<Guid> currentChildProducts, List<Guid> newChildProducts, List<Guid> removeChildProducts)> GetNewAndRemoveChildProductIdsAsync(Guid parentId, List<Guid> requestedChildProductIds)
    {
        var productRelationships = await GetListAsync(
            predicate: x => x.ParentProductId == parentId
        );
        var childProducts = productRelationships.Select(x => x.ChildProductId).ToList();
        var newChildProducts = requestedChildProductIds.Except(childProducts).ToList();
        var removeChildProducts = childProducts.Except(requestedChildProductIds).ToList();
        var currentChildProducts = childProducts.Except(removeChildProducts).ToList();
        return (currentChildProducts, newChildProducts, removeChildProducts);
    }

    public Task<ProductRelationship> GetChildProductByIdAsync(Guid parentId, Guid childProductId)
    {
        var productRelationship = SingleOrDefaultAsync(
            predicate: x => x.ParentProductId == parentId && x.ChildProductId == childProductId
        );
        return productRelationship;
    }
}