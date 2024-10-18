using KALS.API.Models.Product;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface IProductService
{
    Task<IPaginate<GetProductWithCatogoriesResponse>> GetAllProductPagingAsync(int page, int size, ProductFilter? filter, string? sortBy, bool isAsc);
    Task<GetProductDetailResponse> GetProductByIdAsync(Guid id);
    Task<GetProductResponse> CreateProductAsync(CreateProductRequest request);
    Task<GetProductResponse> UpdateProductByIdAsync(Guid id, UpdateProductRequest request);
    Task<GetProductResponse> UpdateProductRelationshipByProductIdAsync(Guid parentId, UpdateChildProductForKitRequest request);
    Task<ICollection<GetProductResponse>> GetChildProductsByParentIdAsync(Guid parentId);
    Task<IPaginate<GetProductResponse>> GetProductByCategoryIdAsync(Guid categoryId, int page, int size);
    
    Task<GetProductResponse> DeleteProductImageById(Guid id);
    
    Task<GetProductResponse> AddProductImageByProductIdAsync(Guid productId,  AddImageProductRequest request);
    
}