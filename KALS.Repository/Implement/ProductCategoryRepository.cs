using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class ProductCategoryRepository: GenericRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(DbContext context) : base(context)
    {
    }

    public async Task<(List<Guid> newProductIds, List<Guid> removeProductIds)> GetNewAndRemoveProductIdsAsync(Guid categoryId, List<Guid> requestedProductIds)
    {
        var productCategories = await GetListAsync(
            predicate: pc => pc.CategoryId == categoryId
        );
        var productIds = productCategories.Select(pc => pc.ProductId).ToList();
        var newProductIds = requestedProductIds.Except(productIds).ToList();
        var removeProductIds = productIds.Except(requestedProductIds).ToList();
        return (newProductIds, removeProductIds);
    }

    public async Task<ICollection<ProductCategory>> GetProductCategoriesByProductIds(List<Guid> productIds)
    {
        var productCategories = await GetListAsync(
            predicate: pc => productIds.Contains(pc.ProductId)
        );
        return productCategories;
    }
}