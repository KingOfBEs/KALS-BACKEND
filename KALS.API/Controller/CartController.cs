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
    [ProducesResponseType(typeof(ICollection<CartModelResponse>), statusCode: StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToCart([FromBody] CartModel request)
    {
        var response = await _cartService.AddToCartAsync(request);
        if (response == null)
        {
            _logger.LogError($"Add to cart failed with {request.ProductId}");
            return Problem($"{MessageConstant.Cart.AddToCartFail}: {request.ProductId}");
        }
        _logger.LogInformation($"Add to cart successful with {request.ProductId}");
        return CreatedAtAction(nameof(AddToCart), response);
    }
    [HttpGet(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModelResponse>), statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartAsync()
    {
        var response = await _cartService.GetCartAsync();
        return Ok(response);
    }
    
    [HttpDelete(ApiEndPointConstant.Cart.CartEndPoint)]
    [ProducesResponseType(typeof(ICollection<CartModelResponse>), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromCartAsync()
    {
        var response = await _cartService.ClearCartAsync();
        if (response == null)
        {
            _logger.LogError($"Clear cart failed");
            return Problem($"{MessageConstant.Cart.CartItemIsNull}");
        }
        _logger.LogInformation($"Remove from cart successful");
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