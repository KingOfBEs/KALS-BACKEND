using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Category;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.API.Services.Implement;

public class CategoryService: BaseService<CategoryService>, ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;
    public CategoryService(ILogger<CategoryService> logger, 
        IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, 
        ICategoryRepository categoryRepository, IProductCategoryRepository productCategoryRepository, IProductRepository productRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _categoryRepository = categoryRepository;
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
    }

    public async Task<IPaginate<CategoryResponse>> GetCategoriesPagingAsync(int page, int size)
    {
        // var categories = await _unitOfWork.GetRepository<Category>().GetPagingListAsync(
        //     selector: c => new CategoryResponse()
        //     {
        //         Id = c.Id,
        //         Name = c.Name,
        //         Description = c.Description,
        //         CreatedAt = c.CreatedAt,
        //         ModifiedAt = c.ModifiedAt
        //     },
        //     page: page,
        //     size: size,
        //     orderBy: c => c.OrderByDescending(c => c.CreatedAt)
        // );
        var categories = await _categoryRepository.GetCategoriesPaginateAsync(page, size);
        var response = _mapper.Map<IPaginate<CategoryResponse>>(categories);
        return response;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(Guid id)
    {
        if (id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Category.CategoryIdNotNull);
        // var category = await _unitOfWork.GetRepository<Category>().SingleOrDefaultAsync(
        //     selector: c => new CategoryResponse()
        //     {
        //         Id = c.Id,
        //         Name = c.Name,
        //         Description = c.Description,
        //         CreatedAt = c.CreatedAt,
        //         ModifiedAt = c.ModifiedAt
        //     },
        //     predicate: c => c.Id == id
        // );
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        var categoryResponse = _mapper.Map<CategoryResponse>(category);
        return categoryResponse;
    }

    public async Task<CategoryResponse> UpdateProductCategoryByCategoryIdAsync(Guid categoryId, UpdateProductCategoryRequest request)
    {
        
        if(categoryId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Category.CategoryIdNotNull);
        // var category = await _unitOfWork.GetRepository<Category>().SingleOrDefaultAsync(
        //     predicate: c => c.Id == categoryId,
        //     include: c => c.Include(c => c.ProductCategories)
        //         .ThenInclude(pc => pc.Product)
        // );
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null) throw new BadHttpRequestException(MessageConstant.Category.CategoryNotFound);
        
        // var currentProductIds = category.ProductCategories
        //     .Select(pc => pc.ProductId)
        //     .ToList();
        // var newProductIds = request.ProductIds.Except(currentProductIds).ToList();
        // var removeProductIds = currentProductIds.Except(request.ProductIds).ToList();
        var (newProductIds, removeProductIds) = await _productCategoryRepository.GetNewAndRemoveProductIdsAsync(categoryId, request.ProductIds);
        foreach (var newProductId in newProductIds)
        {
            var newProduct = await _productRepository.GetProductByIdAsync(newProductId);
            if (newProduct == null)
            {
                throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
            }
        }

        using (var transaction  = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (removeProductIds.Any())
                {
                    // var removeProductCategories = category.ProductCategories
                    //     .Where(pc => removeProductIds.Contains(pc.ProductId))
                    //     .ToList();
                    var removeProductCategories =
                        await _productCategoryRepository.GetProductCategoriesByProductIds(removeProductIds);
                    foreach (var removeProductCategory in removeProductCategories)
                    {
                        _productCategoryRepository.DeleteAsync(removeProductCategory);
                    }
                }
                
                if (newProductIds.Any())
                {
                    foreach (var newProductId in newProductIds)
                    {
                        await _productCategoryRepository.InsertAsync(
                            new ProductCategory()
                            {
                                ProductId = newProductId,
                                CategoryId = categoryId
                            });
                    }
                }
                
                bool isSuccess = await _productCategoryRepository.SaveChangesAsync();
                CategoryResponse response = null;
                transaction.Complete();
                if(isSuccess) response = _mapper.Map<CategoryResponse>(category);
                return response;
            }
            catch (TransactionException ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request)
    {
        if (categoryId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Category.CategoryIdNotNull);
        // var category = await _unitOfWork.GetRepository<Category>().SingleOrDefaultAsync(
        //     predicate: c => c.Id == categoryId
        // );
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null) throw new BadHttpRequestException(MessageConstant.Category.CategoryNotFound);
        category.Name = request.Name;
        category.Description = request.Description;
        category.ModifiedAt = TimeUtil.GetCurrentSEATime();
        
        // _unitOfWork.GetRepository<Category>().UpdateAsync(category);
        _categoryRepository.UpdateAsync(category);
        var isSuccess = await _categoryRepository.SaveChangesAsync();
        CategoryResponse response = null;
        if (isSuccess) response = _mapper.Map<CategoryResponse>(category);
        return response;
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = _mapper.Map<Category>(request);
        category.Id = Guid.NewGuid();
        category.CreatedAt = TimeUtil.GetCurrentSEATime();
        category.ModifiedAt = TimeUtil.GetCurrentSEATime();

        // await _unitOfWork.GetRepository<Category>().InsertAsync(category);
        await _categoryRepository.InsertAsync(category);
        var isSuccess = await _categoryRepository.SaveChangesAsync();
        CategoryResponse response = null;
        if (isSuccess) response = _mapper.Map<CategoryResponse>(category);
        return response;
    }
}