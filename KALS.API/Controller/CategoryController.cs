using KALS.API.Constant;
using KALS.API.Models.Category;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.Category.CategoryEndPoint)]
public class CategoryController: BaseController<CategoryController>
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService, IProductService productService) : base(logger)
    {
        _categoryService = categoryService;
        _productService = productService;
    }
    
    [HttpGet(ApiEndPointConstant.Category.CategoryEndPoint)]
    [ProducesResponseType(typeof(IPaginate<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriesPagingAsync(int page = 1, int size = 30)
    {
        var categories = await _categoryService.GetCategoriesPagingAsync(page, size);
        return Ok(categories);
    }
    [HttpGet(ApiEndPointConstant.Category.CategoryById)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }
    [HttpPatch(ApiEndPointConstant.Category.UpdateProductCategory)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProductCategoryByCategoryIdAsync(Guid id, [FromBody] UpdateProductCategoryRequest request)
    {
        var response = await _categoryService.UpdateProductCategoryByCategoryIdAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Update product category failed with {id}");
            return Problem($"{MessageConstant.Category.UpdateProductCategoryFail}: {id}");
        }
        _logger.LogInformation($"Update product category successful with {id}");
        return Ok(response);
    }

    [HttpPatch(ApiEndPointConstant.Category.CategoryById)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCategoryAsync(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var response = await _categoryService.UpdateCategoryAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Update category failed with {id}");
            return Problem($"{MessageConstant.Category.UpdateCategoryFail}: {id}");
        }

        _logger.LogInformation($"Update category successful with {id}");
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.Category.CategoryEndPoint)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var response = await _categoryService.CreateCategoryAsync(request);
        if (response == null)
        {
            _logger.LogError($"Create new category failed with {request.Name}");
            return Problem($"{MessageConstant.Category.CreateCategoryFail}: {request.Name}");
        }
        _logger.LogInformation($"Create new category successful with {request.Name}");
        return CreatedAtAction(nameof(CreateCategory), response);
    }

    [HttpGet(ApiEndPointConstant.Category.ProductByCategoryId)]
    [ProducesResponseType(typeof(IPaginate<GetProductResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategoryId(Guid id = new Guid(), int page = 1, int size = 30)
    {
        var response = await _productService.GetProductByCategoryIdAsync(id, page, size);
        return Ok(response);
    }
}