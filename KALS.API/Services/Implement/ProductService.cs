using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Cart;
using KALS.API.Models.Category;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KALS.API.Services.Implement;

public class ProductService: BaseService<ProductService>, IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRelationshipRepository _productRelationshipRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IFirebaseService _firebaseService;
    private readonly IRedisService _redisService;
    public ProductService(ILogger<ProductService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, 
        IProductRepository productRepository, ICategoryRepository categoryRepository, IProductRelationshipRepository productRelationshipRepository,
        IProductCategoryRepository productCategoryRepository, IProductImageRepository productImageRepository, IFirebaseService firebaseService,
        IRedisService redisService) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productRelationshipRepository = productRelationshipRepository;
        _productCategoryRepository = productCategoryRepository;
        _productImageRepository = productImageRepository;
        _firebaseService = firebaseService;
        _redisService = redisService;
    }

    public async Task<IPaginate<GetProductWithCatogoriesResponse>> GetAllProductPagingAsync(int page, int size, ProductFilter? filter, string? sortBy, bool isAsc)
    {
        var role = GetRoleFromJwt();
        IPaginate<GetProductWithCatogoriesResponse> response = null;
        switch (role)
        {
            case RoleEnum.Manager:
            case RoleEnum.Staff:
                var products = await _productRepository.GetProductPagingAsync(page, size, filter, sortBy, isAsc);
                response = _mapper.Map<IPaginate<GetProductWithCatogoriesResponse>>(products);
                break;
            default:
                var productsNotHidden = await _productRepository.GetProductNotHiddenPagingAsync(page, size, filter, sortBy, isAsc);
                response = _mapper.Map<IPaginate<GetProductWithCatogoriesResponse>>(productsNotHidden);
                break;
        }
        return response;
    }

    public async Task<GetProductDetailResponse> GetProductByIdAsync(Guid id)
    {
        if(id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var role = GetRoleFromJwt();
        
        var p = await _productRepository.GetProductByIdAsync(id);
        if(p.IsHidden && role != RoleEnum.Manager && role != RoleEnum.Staff)
            throw new BadHttpRequestException(MessageConstant.Product.ProductIsHidden);
        var productResponse = new GetProductDetailResponse()
        {
            
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Quantity = p.Quantity,
            CreatedAt = p.CreatedAt,
            ModifiedAt = p.ModifiedAt,
            IsHidden = p.IsHidden,
            IsKit = p.IsKit,
            ChildProducts = p.ChildProducts.Select(pr => pr.ChildProduct).Select(cp => new ChildProductResponse()
            {
                Id = cp.Id,
                Name = cp.Name,
                Description = cp.Description,
                Quantity = cp.Quantity,
                Price = cp.Price,
                IsHidden = cp.IsHidden,
                IsKit = cp.IsKit,
                CreatedAt = cp.CreatedAt,
                ModifiedAt = cp.ModifiedAt,
                QuantityInKit = cp.ParentProducts.First(pp => pp.ParentProductId == p.Id).Quantity
            }).ToList(),
            ProductImages = p.ProductImages.Select(pi => new ProductImageResponse()
            {
                Id = pi.Id,
                ImageUrl = pi.ImageUrl,
                IsMain = pi.IsMain
            }).ToList(),
            Categories = p.ProductCategories.Select(pc => pc.Category).Select(c => new CategoryResponse()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ModifiedAt = c.ModifiedAt
            }).ToList()
        };
        return productResponse;
    }

    public async Task<GetProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        
        var product = _mapper.Map<Product>(request);
        product.Id = Guid.NewGuid();
        product.CreatedAt = TimeUtil.GetCurrentSEATime();
        product.ModifiedAt = TimeUtil.GetCurrentSEATime();
        decimal expectedPrice = 0;
        if (request.IsKit && request.RequestChildProducts != null)
        {
            foreach (var childProduct in request.RequestChildProducts)
            {
                var requestedChildProduct = await _productRepository.GetProductByIdAsync(childProduct.ChildProductId);
                if (requestedChildProduct == null)
                    throw new BadHttpRequestException(MessageConstant.Product.ChildProductNotFound);
                expectedPrice += requestedChildProduct.Price * childProduct.Quantity;
            }
            if (request.Price > expectedPrice)
            {
                throw new BadHttpRequestException(MessageConstant.Product.PriceMustBeSmallerThanExpectedPrice);
            }
        }

        if (request.CategoryIds != null)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                    throw new BadHttpRequestException(MessageConstant.Category.CategoryNotFound);
            }
        }
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (request.RequestChildProducts != null && request.IsKit)
                {
                    foreach (var childProduct in request.RequestChildProducts)
                    {
                        await _productRelationshipRepository.InsertAsync(new ProductRelationship()
                        {
                            ParentProductId = product.Id,
                            ChildProductId = childProduct.ChildProductId,
                            Quantity = childProduct.Quantity
                        });
                    }
                }

                if (request.CategoryIds != null)
                {
                    foreach (var categoryId in request.CategoryIds)
                    {
                        await _productCategoryRepository.InsertAsync(
                            new ProductCategory()
                            {
                                ProductId = product.Id,
                                CategoryId = categoryId
                            });
                    }
                }

                var mainImageUrl = await _firebaseService.UploadFileToFirebaseAsync(request.MainImage);
                if (!string.IsNullOrEmpty(mainImageUrl))
                {
                    await _productImageRepository.InsertAsync(new ProductImage()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = mainImageUrl,
                        IsMain = true
                    });
                }

                if (request.SecondaryImages != null)
                {
                    var imageUrls = await _firebaseService.UploadFilesToFirebaseAsync(request.SecondaryImages);
                    if (imageUrls.Any())
                    {
                        foreach (var imageUrl in imageUrls)
                        {
                            await _productImageRepository.InsertAsync(new ProductImage()
                            {
                                Id = Guid.NewGuid(),
                                ProductId = product.Id,
                                ImageUrl = imageUrl,
                                IsMain = false
                            });
                        }
                    }
                }
                await _productRepository.InsertAsync(product);
                bool isSuccess = await _productRepository.SaveChangesAsync();
                if (!isSuccess) return null;
                transaction.Complete();
                return _mapper.Map<GetProductResponse>(product);
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
    public async Task<GetProductResponse> UpdateProductByIdAsync(Guid id, UpdateProductRequest request)
    {
        if(id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        
        var product = await _productRepository.GetProductByIdNoIncludeAsync(id);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        product.Name = string.IsNullOrEmpty(request.Name) ? product.Name : request.Name;
        product.Description = string.IsNullOrEmpty(request.Description) ? product.Description : request.Description;
        // product.Price = (int) (request.Price == null ? product.Price : request.Price);
        product.Quantity = (int)(request.Quantity == null ? product.Quantity : request.Quantity);
        product.IsHidden = (bool) (request.IsHidden == null ? product.IsHidden : request.IsHidden);
        product.IsKit = (bool) (request.IsKit == null ? product.IsKit : request.IsKit);
        product.ModifiedAt = TimeUtil.GetCurrentSEATime();
        var productRelationships = await _productRelationshipRepository.GetProductRelationsipByParentId(id);
        if (request.IsKit == false && productRelationships.Any())
        { 
            foreach (var childProduct in productRelationships)
            { 
                _productRelationshipRepository.DeleteAsync(childProduct); 
            }
            product.Price = (int) (request.Price == null ? product.Price : request.Price);
                    
            _productRepository.UpdateAsync(product);
            GetProductResponse accessoryProduct = null;
            var isAccessorySuccess = await _productRepository.SaveChangesAsync();
            if (isAccessorySuccess) accessoryProduct = _mapper.Map<GetProductResponse>(product);
            return accessoryProduct;
        }
        decimal expectedPrice = 0;
        if (request.ChildProducts!.Any())
        {
            foreach (var childProductRequest in request.ChildProducts!)
            {
                var childProduct =
                    await _productRepository.GetProductByIdNoIncludeAsync(childProductRequest.ChildProductId);
                expectedPrice += childProduct.Price * childProductRequest.Quantity;
            }
            if (request.Price > expectedPrice)
                throw new BadHttpRequestException(MessageConstant.Product.PriceMustBeSmallerThanExpectedPrice);
        }
        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                product.Price = (int) (request.Price == null ? product.Price : request.Price);
                _productRepository.UpdateAsync(product);
                if (product.IsKit && request.ChildProducts != null)
                {
                    decimal expectedPriceKit = 0;
                    foreach (var childProductRequest in request.ChildProducts)
                    {
                        var childProduct =
                            await _productRepository.GetProductByIdNoIncludeAsync(childProductRequest.ChildProductId);
                        if (childProduct == null)
                            throw new BadHttpRequestException(MessageConstant.Product.ChildProductNotFound);
                        expectedPrice += childProduct.Price * childProductRequest.Quantity;
                    }
                    if (request.Price > expectedPrice)
                        throw new BadHttpRequestException(MessageConstant.Product.PriceMustBeSmallerThanExpectedPrice);
                    
                    var childProductIds = request.ChildProducts.Select(r => r.ChildProductId).ToList();
                    if (!request.ChildProducts.Any()) throw new BadHttpRequestException(MessageConstant.Product.ChildProductIdNotNull);
                    var (currentChildProductIds, addChildProductIds, removeChildProductIds) = 
                        await _productRelationshipRepository.GetNewAndRemoveChildProductIdsAsync(id, childProductIds);
                    
                        if (currentChildProductIds.Any())
                        {
                            foreach (var currentChildProduct in currentChildProductIds)
                            {
                                var currentChildProductRelationship = await _productRelationshipRepository.GetChildProductByIdAsync(id, currentChildProduct);
                                currentChildProductRelationship.Quantity = 
                                    request.ChildProducts.First(r => r.ChildProductId == currentChildProduct).Quantity;
                                _productRelationshipRepository.UpdateAsync(currentChildProductRelationship);
                            }
                        }
                        if (removeChildProductIds.Any())
                        {
                            foreach (var removeChildProductId in removeChildProductIds)
                            {
                                var removeChildProduct =
                                    await _productRelationshipRepository.GetChildProductByIdAsync(id,
                                        removeChildProductId);
                                _productRelationshipRepository.DeleteAsync(removeChildProduct);
                            }
                        }
                        if (addChildProductIds.Any())
                        {
                            foreach (var addChildProductId in addChildProductIds)
                            {
                                await _productRelationshipRepository.InsertAsync(
                                    new ProductRelationship()
                                    {
                                        ParentProductId = id,
                                        ChildProductId = addChildProductId,
                                        Quantity = request.ChildProducts.First(r => r.ChildProductId == addChildProductId).Quantity
                                    });
                            } 
                        }
                }
                bool isSuccess = await _productRelationshipRepository.SaveChangesAsync();
                transactionScope.Complete();
                GetProductResponse productResponse = null;
                if (isSuccess)
                {
                    productResponse = _mapper.Map<GetProductResponse>(product);
                    var cartKeys = await _redisService.GetListAsync("AllCartKeys");
                    if (cartKeys.Any())
                    {
                        foreach (var cartKey in cartKeys)
                        {
                            var cartJson = await _redisService.GetStringAsync(cartKey);
                            var cart = JsonConvert.DeserializeObject<List<CartModelResponse>>(cartJson);
                            foreach (var cartItem in cart)
                            {
                                if (cartItem.ProductId == product.Id)
                                {
                                    
                                    if (cartItem.Quantity > product.Quantity || product.IsHidden)
                                    {
                                        cart.Remove(cartItem);
                                        break;
                                    }
                                    cartItem.Name = product.Name;
                                    cartItem.Description = product.Description;
                                    cartItem.Price = product.Price;
                                    cartItem.MainImage = product.ProductImages?.Where(pi => pi.IsMain == true).FirstOrDefault()?.ImageUrl;
                                    cartItem.ProductQuantity = product.Quantity;
                                }
                            }
                            if (!cart.Any())
                            {
                                await _redisService.RemoveKeyAsync(cartKey);
                                await _redisService.RemoveFromListAsync("AllCartKeys", cartKey);
                            }
                            else
                            {
                                await _redisService.SetStringAsync(cartKey, JsonConvert.SerializeObject(cart));
                            }
                        }
                    }
                }
                return productResponse;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }

    public async Task<GetProductResponse> UpdateProductRelationshipByProductIdAsync(Guid parentId, ICollection<UpdateChildProductForKitRequest> request)
    {
        if(parentId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        if (!request.Any())
            throw new BadHttpRequestException(MessageConstant.Product.ChildProductIdNotNull);
        
        var parentProduct = await _productRepository.GetProductByIdAsync(parentId);
        if(parentProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        if (parentProduct.IsKit == false) throw new BadHttpRequestException(MessageConstant.Product.ProductIsNotKit);
        var childProductIds = request.Select(r => r.ChildProductId).ToList();
        var (currentChildProductIds, addChildProductIds, removeChildProductIds) = 
            await _productRelationshipRepository.GetNewAndRemoveChildProductIdsAsync(parentId, childProductIds);
        foreach (var addChildProductId in addChildProductIds)
        {
            var addChildProduct = await _productRepository.GetProductByIdAsync(addChildProductId);
            if(addChildProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ChildProductNotFound);
        }
        if (!currentChildProductIds.Any() && !removeChildProductIds.Any() && !addChildProductIds.Any())
        {
            return _mapper.Map<GetProductResponse>(parentProduct);
        }
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (currentChildProductIds.Any())
                {
                    foreach (var currentChildProduct in currentChildProductIds)
                    {
                        var currentChildProductRelationship = await _productRelationshipRepository.GetChildProductByIdAsync(parentId, currentChildProduct);
                        currentChildProductRelationship.Quantity = 
                            request.First(r => r.ChildProductId == currentChildProduct).Quantity;
                        _productRelationshipRepository.UpdateAsync(currentChildProductRelationship);
                    }
                }
                if (removeChildProductIds.Any())
                {
                    foreach (var removeChildProductId in removeChildProductIds)
                    {
                        var removeChildProduct =
                            await _productRelationshipRepository.GetChildProductByIdAsync(parentId,
                                removeChildProductId);
                        _productRelationshipRepository.DeleteAsync(removeChildProduct);
                    }
                }
                if (addChildProductIds.Any())
                {
                    foreach (var addChildProductId in addChildProductIds)
                    {
                        await _productRelationshipRepository.InsertAsync(
                            new ProductRelationship()
                            {
                                ParentProductId = parentId,
                                ChildProductId = addChildProductId,
                                Quantity = request.First(r => r.ChildProductId == addChildProductId).Quantity
                            });
                    }
                }
                
                bool isSuccess = await _productRelationshipRepository.SaveChangesAsync();
                transaction.Complete();
                GetProductResponse productResponse = null;
                if (isSuccess)
                {
                    productResponse = _mapper.Map<GetProductResponse>(parentProduct);
                }
                return productResponse;
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

    public async Task<ICollection<GetProductResponse>> GetChildProductsByParentIdAsync(Guid parentId)
    {
        if(parentId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ParentProductIdNotNull);
        
        var childProducts = await _productRepository.GetListProductsByParentIdAsync(parentId);
        var childProductResponses = _mapper.Map<ICollection<GetProductResponse>>(childProducts);
        return childProductResponses;
    }

    public async Task<IPaginate<GetProductResponse>> GetProductByCategoryIdAsync(Guid categoryId, int page, int size)
    {
        if (categoryId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Category.CategoryIdNotNull);
        
        var products = await _productRepository.GetProductsPagingByCategoryId(categoryId, page, size);
        var productResponses = _mapper.Map<IPaginate<GetProductResponse>>(products);
        return productResponses;
    }

    public async Task<GetProductResponse> DeleteProductImageById(Guid id)
    {
        if (id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.ProductImage.ProductImageIdNotNull);
        var productImage = await _productImageRepository.GetProductImageByIdAsync(id);
        if (productImage == null) throw new BadHttpRequestException(MessageConstant.ProductImage.ProductImageNotFound);
        _productImageRepository.DeleteAsync(productImage);
        
        bool isSuccess = await _productImageRepository.SaveChangesAsync();
        GetProductResponse productResponse = null;
        if (isSuccess) productResponse = _mapper.Map<GetProductResponse>(productImage.Product);
        return productResponse;
    }

    public async Task<GetProductResponse> UpdateProductImageByProductIdAsync(Guid productId,
        ICollection<AddImageProductRequest> request)
    {
        if (productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var product = await _productRepository.GetProductByIdAsync(productId);
        if (product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        
        if(request.Where(pi => pi.IsMain).ToList().Count != 1)
            throw new BadHttpRequestException(MessageConstant.ProductImage.WrongMainImageQuantity);
        var requestProductImagesId = request
            .Where(pi => pi.Id != null)
            .Select(pi => pi.Id!.Value).ToList();
        
        var removedProductImageIds = await _productImageRepository.GetRemovedProductImageIdsAsync(productId, requestProductImagesId);
        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                foreach (var removeProductImageId in removedProductImageIds)
                {
                    var productImage = await _productImageRepository.GetProductImageByIdAsync(removeProductImageId);
                    _productImageRepository.DeleteAsync(productImage);
                }
                foreach (var imageProduct in request)
                {
                    if (imageProduct.Id == null)
                    {
                        var imageUrl = await _firebaseService.UploadFileToFirebaseAsync(imageProduct.ImageUrl);
                        if (string.IsNullOrEmpty(imageUrl))
                            throw new BadHttpRequestException(MessageConstant.ProductImage.UploadImageFail);
                        var newProductImage = new ProductImage()
                        {
                            Id = Guid.NewGuid(),
                            IsMain = imageProduct.IsMain,
                            ImageUrl = imageUrl,
                            ProductId = product.Id
                        };
                        await _productImageRepository.InsertAsync(newProductImage);
                    }
                    else
                    {
                        var productImage = _mapper.Map<ProductImage>(imageProduct);
                        productImage.Id = imageProduct.Id!.Value;
                        productImage.ProductId = product.Id;
                        _productImageRepository.UpdateAsync(productImage);
                    }
                }
                bool isSuccess = await _productImageRepository.SaveChangesAsync();
                transactionScope.Complete();
                GetProductResponse productResponse = null;
                if (isSuccess) productResponse = _mapper.Map<GetProductResponse>(product);
                return productResponse;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}