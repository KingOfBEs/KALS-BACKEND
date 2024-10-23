using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Models.Lab;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.Product.ProductEndpoint)]
public class ProductController : BaseController<ProductController>
{
    private readonly IProductService _productService;
    private readonly ILabService _labService;
    private readonly ICartService _cartService;
    public ProductController(ILogger<ProductController> logger, IProductService productService, ILabService labService, ICartService cartService) : base(logger)
    {
        _productService = productService;
        _labService = labService;
        _cartService = cartService;
    }
    [HttpGet(ApiEndPointConstant.Product.ProductEndpoint)]
    [ProducesResponseType(typeof(IPaginate<GetProductWithCatogoriesResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProduct(int page = 1, int size = 30,[FromQuery] ProductFilter? filter = null, string? sortBy = null, bool isAsc = true)
    {
        var response = await _productService.GetAllProductPagingAsync(page, size, filter, sortBy, isAsc);
        return Ok(response);
    }
    [HttpGet(ApiEndPointConstant.Product.ProductById)]
    [ProducesResponseType(typeof(GetProductDetailResponse), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var response = await _productService.GetProductByIdAsync(id);
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.Product.ProductEndpoint)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var response = await _productService.CreateProductAsync(request);
        if (response == null)
        {
            _logger.LogError($"Create new product failed with {request.Name}");
            return Problem($"{MessageConstant.Product.CreateProductFail}: {request.Name}");
        }
        _logger.LogInformation($"Create new product successful with {request.Name}");
        return CreatedAtAction(nameof(CreateProduct), response);
    }
    [HttpPatch(ApiEndPointConstant.Product.ProductById)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateProductById(Guid id, [FromBody] UpdateProductRequest request)
    {
        var response = await _productService.UpdateProductByIdAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Update product failed with {id}");
            return Problem($"{MessageConstant.Product.UpdateProductFail}: {id}");
        }
        _logger.LogInformation($"Update product successful with {id}");
        return Ok(response);
    }
    [HttpPatch(ApiEndPointConstant.Product.UpdateProductRelationship)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> UpdateProductRelationshipByProductId(Guid id, [FromBody] UpdateChildProductForKitRequest request)
    {
        var response = await _productService.UpdateProductRelationshipByProductIdAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Update product relationship failed with {id}");
            return Problem($"{MessageConstant.Product.UpdateProductRelationshipFail}: {id}");
        }
        _logger.LogInformation($"Update product relationship successful with {id}");
        return Ok(response);
    }
    [HttpPatch(ApiEndPointConstant.Product.LabToProduct)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> AssignLabToProduct(Guid id, [FromBody] AssignLabsToProductRequest request)
    {
        var response = await _labService.AssignLabToProductAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Assign lab to product failed with {id}");
            return Problem($"{MessageConstant.Lab.AssignLabToProductFail}: {id}");
        }
        _logger.LogInformation($"Assign lab to product successful with {id}");
        return Ok(response);
    }

    [HttpGet(ApiEndPointConstant.Product.LabToProduct)]
    [ProducesResponseType(typeof(ProductWithLabResponse), statusCode: StatusCodes.Status200OK)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> GetLabsByProductId(Guid id)
    {
        var response = await _labService.GetLabsByProductIdAsync(id);
        return Ok(response);
    }
    [HttpDelete(ApiEndPointConstant.Product.CartByProductId)]
    [ProducesResponseType(typeof(ICollection<CartModelResponse>), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff, RoleEnum.Member)]
    public async Task<IActionResult> RemoveFromCartAsync(Guid id)
    {
        var response = await _cartService.RemoveFromCartAsync(id);
        if (response == null)
        {
            _logger.LogError($"Remove from cart failed with {id}");
            return Problem($"{MessageConstant.Cart.RemoveFromCartFail}: {id}");
        }
        _logger.LogInformation($"Remove from cart successful with {id}");
        return Ok(response);
    }
    [HttpPost(ApiEndPointConstant.Product.ProductImage)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> AddProductImage(Guid id, [FromBody] AddImageProductRequest request)
    {
        var response = await _productService.AddProductImageByProductIdAsync(id, request);
        if (response == null)
        {
            _logger.LogError($"Add product image failed with {id}");
            return Problem($"{MessageConstant.ProductImage.AddProductImageFail}: {id}");
        }
        _logger.LogInformation($"Add product image successful with {id}");
        return CreatedAtAction(nameof(AddProductImage), response);
    }
    
}