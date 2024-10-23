using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
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

namespace KALS.API.Services.Implement;

public class ProductService: BaseService<ProductService>, IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRelationshipRepository _productRelationshipRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IFirebaseService _firebaseService;
    public ProductService(ILogger<ProductService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, 
        IProductRepository productRepository, ICategoryRepository categoryRepository, IProductRelationshipRepository productRelationshipRepository,
        IProductCategoryRepository productCategoryRepository, IProductImageRepository productImageRepository, IFirebaseService firebaseService) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productRelationshipRepository = productRelationshipRepository;
        _productCategoryRepository = productCategoryRepository;
        _productImageRepository = productImageRepository;
        _firebaseService = firebaseService;
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
        var p = await _productRepository.GetProductByIdAsync(id);
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
            ChildProducts = p.ChildProducts.Select(pr => pr.ChildProduct).Select(cp => new GetProductResponse()
            {
                Id = cp.Id,
                Name = cp.Name,
                Description = cp.Description,
                Quantity = cp.Quantity,
                Price = cp.Price,
                IsHidden = cp.IsHidden,
                IsKit = cp.IsKit,
                CreatedAt = cp.CreatedAt,
                ModifiedAt = cp.ModifiedAt
            }).ToList(),
            ProductImages = p.ProductImages.Select(pi => new ProductImageResponse()
            {
                Id = pi.Id,
                ImageUrl = pi.ImageUrl,
                isMain = pi.isMain
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
        if (request.ChildProductIds != null)
        {
            foreach (var childProductId in request.ChildProductIds)
            {
                var requestedChildProduct = await _productRepository.GetProductByIdAsync(childProductId);
                if (requestedChildProduct == null)
                    throw new BadHttpRequestException(MessageConstant.Product.ChildProductNotFound);
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
                if (request.ChildProductIds != null)
                {
                    foreach (var childProductId in request.ChildProductIds)
                    {
                        await _productRelationshipRepository.InsertAsync(new ProductRelationship()
                        {
                            ParentProductId = product.Id,
                            ChildProductId = childProductId
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
                        isMain = true
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
                                isMain = false
                            });
                        }
                    }
                }
                await _productRepository.InsertAsync(product);
                bool isSuccess = await _productRepository.SaveChangesAsync();
                if (!isSuccess) return null;
                transaction.Complete();
                return _mapper.Map<GetProductResponse>(product);;
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
        
        var product = await _productRepository.GetProductByIdAsync(id);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        product.Name = string.IsNullOrEmpty(request.Name) ? product.Name : request.Name;
        product.Description = string.IsNullOrEmpty(request.Description) ? product.Description : request.Description;
        product.Price = (int) (request.Price == null ? product.Price : request.Price);
        product.Quantity = (int)(request.Quantity == null ? product.Quantity : request.Quantity);
        product.IsHidden = (bool) (request.IsHidden == null ? product.IsHidden : request.IsHidden);
        product.IsKit = (bool) (request.IsKit == null ? product.IsKit : request.IsKit);
        if (request.IsKit == false && product.ChildProducts!.Any())
        { 
            foreach (var childProduct in product.ChildProducts!)
            { 
                _productRelationshipRepository.DeleteAsync(childProduct); 
            }
        }
        product.ModifiedAt = TimeUtil.GetCurrentSEATime();
        
        // _unitOfWork.GetRepository<Product>().UpdateAsync(product);
        _productRepository.UpdateAsync(product);
        bool isSuccess = await _productRepository.SaveChangesAsync();
        GetProductResponse productResponse = null;
        if (isSuccess) productResponse = _mapper.Map<GetProductResponse>(product);
        return productResponse;
    }

    public async Task<GetProductResponse> UpdateProductRelationshipByProductIdAsync(Guid parentId, UpdateChildProductForKitRequest request)
    {
        if(parentId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        if (!request.ChildProductIds.Any())
            throw new BadHttpRequestException(MessageConstant.Product.ChildProductIdNotNull);
        
        var parentProduct = await _productRepository.GetProductByIdAsync(parentId);
        
        if(parentProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        if (parentProduct.IsKit == false) throw new BadHttpRequestException(MessageConstant.Product.ProductIsNotKit);
        // var currentChildProductIds = parentProduct.ChildProducts!.Select(pr => pr.ChildProductId).ToList();
        // var addChildProductIds = request.ChildProductIds.Except(currentChildProductIds);
        // var removeChildProductIds = currentChildProductIds.Except(request.ChildProductIds);
        var (addChildProductIds, removeChildProductIds) = await _productRelationshipRepository.GetNewAndRemoveChildProductIdsAsync(parentId, request.ChildProductIds);
        foreach (var addChildProductId in addChildProductIds)
        {
            var addChildProduct = await _productRepository.GetProductByIdAsync(addChildProductId);
            if(addChildProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ChildProductNotFound);
        }

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (removeChildProductIds.Any())
                {
                    foreach (var removeChildProductId in removeChildProductIds)
                    {
                        // var removeChildProduct = parentProduct.ChildProducts
                        //     .FirstOrDefault(pr => pr.ChildProductId == removeChildProductId);
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
                        // var addChildProduct = await _productRepository.GetProductByIdAsync(addChildProductId);
                        await _productRelationshipRepository.InsertAsync(
                            new ProductRelationship()
                            {
                                ParentProductId = parentId,
                                ChildProductId = addChildProductId
                            });
                    }
                }
                _productRepository.UpdateAsync(parentProduct);
                bool isSuccess = await _productRepository.SaveChangesAsync();
                
                transaction.Complete();
                GetProductResponse productResponse = null;
                if (isSuccess) productResponse = _mapper.Map<GetProductResponse>(parentProduct);
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

    public async Task<GetProductResponse> AddProductImageByProductIdAsync(Guid productId, AddImageProductRequest request)
    {
        if (productId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        var product = await _productRepository.GetProductByIdAsync(productId);
        if (product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        if (request.IsMain)
        {
            var productImages = await _productImageRepository.GetProductImagesByProductId(productId);
            if(productImages.Any(pi => pi.isMain)) throw new BadHttpRequestException(MessageConstant.ProductImage.MainImageExist);
        }
        var imageUrl = await _firebaseService.UploadFileToFirebaseAsync(request.Image);
        if (string.IsNullOrEmpty(imageUrl)) throw new BadHttpRequestException(MessageConstant.ProductImage.UploadImageFail);
        var newProductImage = new ProductImage()
        {
            Id = Guid.NewGuid(),
            isMain = request.IsMain,
            ImageUrl = imageUrl,
            ProductId = product.Id
        };
        await _productImageRepository.InsertAsync(newProductImage);
        GetProductResponse response = null;
        var isSuccess = await _productImageRepository.SaveChangesAsync();
        if (isSuccess) response = _mapper.Map<GetProductResponse>(product);
        return response;
    }
}