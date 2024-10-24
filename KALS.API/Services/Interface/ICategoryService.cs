using KALS.API.Models.Category;
using KALS.API.Models.Product;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.API.Services.Interface;

public interface ICategoryService
{
    Task<IPaginate<CategoryResponse>> GetCategoriesPagingAsync(int page, int size, CategoryFilter? filter);
    Task<CategoryResponse> GetCategoryByIdAsync(Guid id);
    
    Task<CategoryResponse> UpdateProductCategoryByCategoryIdAsync(Guid categoryId, UpdateProductCategoryRequest request);
    
    Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request);
    
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    
}