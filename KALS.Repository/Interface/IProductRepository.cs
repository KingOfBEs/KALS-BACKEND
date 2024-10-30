using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface IProductRepository: IGenericRepository<Product>
{
    Task<Product> GetProductByIdAsync(Guid id);
    
    Task<Product> GetProductByIdNoIncludeAsync(Guid id);
    
    Task<IPaginate<Product>> GetProductNotHiddenPagingAsync(int page, int size, IFilter<Product>? filter, string? sortBy,
        bool isAsc);
    Task<IPaginate<Product>> GetProductPagingAsync(int page, int size, IFilter<Product>? filter, string? sortBy, bool isAsc);
    
    Task<ICollection<Product>> GetListProductsByParentIdAsync(Guid parentId);
    
    Task<IPaginate<Product>> GetProductsPagingByCategoryId(Guid categoryId, int page, int size);
    
    Task<(List<Guid> newChildProducts, List<Guid> removeChildProducts)> GetNewAndRemoveChildProductIdsAsync(Guid parentId, List<Guid> requestedChildProductIds);
    
}