using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface IProductRelationshipRepository: IGenericRepository<ProductRelationship>
{
    Task<(List<Guid> newChildProducts, List<Guid> removeChildProducts)> GetNewAndRemoveChildProductIdsAsync(Guid parentId, List<Guid> requestedChildProductIds);
    Task<ProductRelationship> GetChildProductByIdAsync(Guid parentId, Guid childProductId);
}