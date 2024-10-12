using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KALS.API.Services.Implement;

public class CartService: BaseService<CartService>, ICartService
{
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    public CartService(ILogger<CartService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, 
        IConfiguration configuration, IProductRepository productRepository, IUserRepository userRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<ICollection<CartModelResponse>> AddToCartAsync(CartModel request)
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        // var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(
        //     predicate:x => x.Id == userId
        // );
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);

        // var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     predicate: x => x.Id == request.ProductId
        // );
        var product = await _productRepository.GetProductByIdAsync(request.ProductId);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        
        var response = _mapper.Map<CartModelResponse>(product);
        response.Quantity = request.Quantity;
        response.ProductId = product.Id;
        
        var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        var db = redis.GetDatabase();
        var key = "Cart:" + userId;
        var cartData =  await db.StringGetAsync(key);
        List<CartModelResponse> cart = new();

        if (cartData.IsNullOrEmpty)
        {
            cart = new List<CartModelResponse>();
        }
        else
        {
            cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        }
        var existedProduct = cart.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (existedProduct != null)
        {
            existedProduct.Quantity += request.Quantity;
        }
        else
        {
            cart.Add(response);
        }
        var updatedCart = JsonConvert.SerializeObject(cart);
        List<CartModelResponse> result = null;
        var isSuccess = await db.StringSetAsync(key, updatedCart);
        if(isSuccess) result = cart;
        return result;
    }

    public async Task<ICollection<CartModelResponse>> GetCartAsync()
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        var db = redis.GetDatabase();
        var key = "Cart:" + userId;
        
        var cartData = await db.StringGetAsync(key);

        if (cartData.IsNullOrEmpty)
        {
            return new List<CartModelResponse>();
        }
        var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        return cart;
    }

    public async Task<ICollection<CartModelResponse>> RemoveFromCartAsync(Guid productId)
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);

        if (productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        var db = redis.GetDatabase();
        var key = "Cart:" + userId;
        var cartData = await db.StringGetAsync(key);
        
        if (cartData.IsNullOrEmpty)
        {
            return new List<CartModelResponse>();
        }
        var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        var product = cart.FirstOrDefault(x => x.ProductId == productId);

        if (product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        cart.Remove(product);
        if (!cart.Any())
        {
            await db.KeyDeleteAsync(key);
        }
        else
        {
            var updatedCart = JsonConvert.SerializeObject(cart);
            await db.StringSetAsync(key, updatedCart);
        }

        return cart;
    }

    public async Task<ICollection<CartModelResponse>> UpdateQuantityAsync(CartModel request)
    {
        if (request.ProductId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        if (request.Quantity <= 0) throw new BadHttpRequestException(MessageConstant.Cart.QuantityMustBeGreaterThanZero);
        
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        
        // var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     predicate: x => x.Id == request.ProductId
        // );
        var product = await _productRepository.GetProductByIdAsync(request.ProductId);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        
        var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        var db = redis.GetDatabase();
        var key = "Cart:" + userId;
        var cartData = await db.StringGetAsync(key);
        
        if (cartData.IsNullOrEmpty)
        {
            return new List<CartModelResponse>();
        }
        var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        var existedProduct = cart.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (existedProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        existedProduct.Quantity = request.Quantity;
        var updatedCart = JsonConvert.SerializeObject(cart);
        await db.StringSetAsync(key, updatedCart);
        return cart;
    }

    public async Task<ICollection<CartModelResponse>> ClearCartAsync()
    {
        var userId = GetUserIdFromJwt();
        if (userId == Guid.Empty) throw new UnauthorizedAccessException(MessageConstant.User.UserNotFound);
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) throw new BadHttpRequestException(MessageConstant.User.UserNotFound);
        var redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
        var db = redis.GetDatabase();
        var key = "Cart:" + userId;
        var cartData = await db.StringGetAsync(key);
        
        if (cartData.IsNullOrEmpty)
        {
            return null;
        }
        var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartData);
        cart.Clear();
        if (!cart.Any())
        {
            await db.KeyDeleteAsync(key);
        }
        return cart;
    }
}