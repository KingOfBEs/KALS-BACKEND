using KALS.API.Constant;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.ProductImage.ProductImageEndPoint)]
public class ProductImageController: BaseController<ProductImageController>
{
    private readonly IProductService _productService;
    public ProductImageController(ILogger<ProductImageController> logger, IProductService productService) : base(logger)
    {
        _productService = productService;
    }
    
    [HttpDelete(ApiEndPointConstant.ProductImage.ProductImageById)]
    [ProducesResponseType(typeof(GetProductResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    [CustomAuthorize(RoleEnum.Manager, RoleEnum.Staff)]
    public async Task<IActionResult> DeleteProductImageById(Guid id)
    {
        var response = await _productService.DeleteProductImageById(id);
        if (response == null)
        {
            _logger.LogError($"Delete product image failed with {id}");
            return Problem($"{MessageConstant.ProductImage.DeleteProductImageFail}: {id}");
        }
        _logger.LogInformation($"Delete product image successful with {id}");
        return Ok(response);
    }
}