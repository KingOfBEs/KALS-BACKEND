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

    public async Task<(List<Guid> newChildProducts, List<Guid> removeChildProducts)> GetNewAndRemoveChildProductIdsAsync(Guid parentId, List<Guid> requestedChildProductIds)
    {
        var productRelationships = await GetListAsync(
            predicate: x => x.ParentProductId == parentId
        );
        var newChildProducts = requestedChildProductIds.Except(productRelationships.Select(x => x.ChildProductId)).ToList();
        var removeChildProducts = productRelationships.Select(x => x.ChildProductId).Except(requestedChildProductIds).ToList();
        return (newChildProducts, removeChildProducts);
    }

    public Task<ProductRelationship> GetChildProductByIdAsync(Guid parentId, Guid childProductId)
    {
        var productRelationship = SingleOrDefaultAsync(
            predicate: x => x.ParentProductId == parentId && x.ChildProductId == childProductId
        );
        return productRelationship;
    }
}