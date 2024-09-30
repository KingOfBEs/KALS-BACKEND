using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Services.Interface;
using KALS.API.Validator;
using KALS.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;

[ApiController]
[Route(ApiEndPointConstant.Cart.CartEndPoint)]
public class CartController: BaseController<CartController>
{
    private readonly ICartService _cartService;
    public CartController(ILogger<CartController> logger, ICartService cartService) : base(logger)
    {
        _cartService = cartService;
    }
    
    [HttpPost(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModel>), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToCartAsync([FromBody] CartModel request)
    {
        var response = await _cartService.AddToCartAsync(request);
        if (response == null)
        {
            _logger.LogError($"Add to cart failed with {request.ProductId}");
            return Problem($"{MessageConstant.Cart.AddToCartFail}: {request.ProductId}");
        }
        _logger.LogInformation($"Add to cart successful with {request.ProductId}");
        return Ok(response);
    }
    [HttpGet(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModel>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartAsync()
    {
        var response = await _cartService.GetCartAsync();
        return Ok(response);
    }
    
    [HttpDelete(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModel>), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromCartAsync([FromQuery] Guid productId)
    {
        var response = await _cartService.RemoveFromCartAsync(productId);
        if (response == null)
        {
            _logger.LogError($"Remove from cart failed with {productId}");
            return Problem($"{MessageConstant.Cart.RemoveFromCartFail}: {productId}");
        }
        _logger.LogInformation($"Remove from cart successful with {productId}");
        return Ok(response);
    }
    [HttpPatch(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModelResponse>), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQuantityAsync([FromBody] CartModel request)
    {
        var response = await _cartService.UpdateQuantityAsync(request);
        if (response == null)
        {
            _logger.LogError($"Update quantity failed with {request.ProductId}");
            return Problem($"{MessageConstant.Cart.UpdateQuantityFail}: {request.ProductId}");
        }
        _logger.LogInformation($"Update quantity successful with {request.ProductId}");
        return Ok(response);
    }
}