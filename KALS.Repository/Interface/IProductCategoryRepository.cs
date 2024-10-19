using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface IProductCategoryRepository: IGenericRepository<ProductCategory>
{
    Task<(List<Guid> newProductIds, List<Guid> removeProductIds)> GetNewAndRemoveProductIdsAsync(Guid categoryId, List<Guid> requestedProductIds);
    
    Task<ICollection<ProductCategory>> GetProductCategoriesByProductIds(List<Guid> productIds);
}