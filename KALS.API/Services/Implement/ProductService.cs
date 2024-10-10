using System.Transactions;
using AutoMapper;
using KALS.API.Constant;
using KALS.API.Models.Category;
using KALS.API.Models.Product;
using KALS.API.Services.Interface;
using KALS.API.Utils;
using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
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
    public ProductService(ILogger<ProductService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, 
        IProductRepository productRepository, ICategoryRepository categoryRepository, IProductRelationshipRepository productRelationshipRepository,
        IProductCategoryRepository productCategoryRepository, IProductImageRepository productImageRepository) : base(logger, mapper, httpContextAccessor, configuration)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productRelationshipRepository = productRelationshipRepository;
        _productCategoryRepository = productCategoryRepository;
        _productImageRepository = productImageRepository;
    }

    public async Task<IPaginate<GetProductWithCatogoriesResponse>> GetAllProductPagingAsync(int page, int size, ProductFilter filter, string? sortBy, bool isAsc)
    {
        // var products = await _unitOfWork.GetRepository<Product>().GetPagingListAsync(
        //     selector: p => new GetProductWithCatogoriesResponse()
        //     {
        //         Id = p.Id,
        //         Name = p.Name,
        //         Description = p.Description,
        //         Price = p.Price,
        //         Quantity = p.Quantity,
        //         CreatedAt = p.CreatedAt,
        //         ModifiedAt = p.ModifiedAt,
        //         IsHidden = p.IsHidden,
        //         IsKit = p.IsKit,
        //         Categories = p.ProductCategories.Any(pc => pc.ProductId == p.Id) ? p.ProductCategories.Select(pc => new CategoryResponse()
        //         {
        //             Id = pc.Category.Id,
        //             Name = pc.Category.Name,
        //             Description = pc.Category.Description,
        //             CreatedAt = pc.Category.CreatedAt,
        //             ModifiedAt = pc.Category.ModifiedAt,
        //         }).ToList() : null,
        //         ProductImages = p.ProductImages.Select(pi => new ProductImageResponse()
        //         {
        //             Id = pi.Id,
        //             ImageUrl = pi.ImageUrl,
        //             isMain = pi.isMain
        //         }).ToList()
        //     },
        //     predicate: p => !p.IsHidden,
        //     // orderBy: p => p.OrderByDescending(p => p.CreatedAt),
        //     page: page,
        //     size: size,
        //     include: p => p.Include(p => p.ProductCategories)
        //         .ThenInclude(pc => pc.Category),
        //     filter: filter,
        //     sortBy: sortBy,
        //     isAsc: isAsc
        // );
        var products = await _productRepository.GetProductPagingAsync(page, size, filter, sortBy, isAsc);
        var productResponse = _mapper.Map<IPaginate<GetProductWithCatogoriesResponse>>(products);
        return productResponse;
    }

    public async Task<GetProductDetailResponse> GetProductByIdAsync(Guid id)
    {
        if(id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        // var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     selector: p => new GetProductDetailResponse()
        //     {
        //         Id = p.Id,
        //         Name = p.Name,
        //         Description = p.Description,
        //         Price = p.Price,
        //         Quantity = p.Quantity,
        //         CreatedAt = p.CreatedAt,
        //         ModifiedAt = p.ModifiedAt,
        //         IsHidden = p.IsHidden,
        //         IsKit = p.IsKit,
        //         ChildProducts = p.ChildProducts.Select(pr => pr.ChildProduct).Select(cp => new GetProductResponse()
        //         {
        //             Id = cp.Id,
        //             Name = cp.Name,
        //             Description = cp.Description,
        //             Quantity = cp.Quantity,
        //             Price = cp.Price,
        //             IsHidden = cp.IsHidden,
        //             IsKit = cp.IsKit,
        //             CreatedAt = cp.CreatedAt,
        //             ModifiedAt = cp.ModifiedAt
        //         }).ToList(),
        //         ProductImages = p.ProductImages.Select(pi => new ProductImageResponse()
        //         {
        //             Id = pi.Id,
        //             ImageUrl = pi.ImageUrl,
        //             isMain = pi.isMain
        //         }).ToList()
        //     },
        //     predicate: p => p.Id == id,
        //     include: p => p.Include(p => p.ChildProducts)
        //         .ThenInclude(cp => cp.ChildProduct)
        // );
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
            }).ToList()
        };
        return productResponse;
    }

    public async Task<GetProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
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
                        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                        if (category == null)
                            throw new BadHttpRequestException(MessageConstant.Category.CategoryNotFound);
                        await _productCategoryRepository.InsertAsync(
                            new ProductCategory()
                            {
                                ProductId = product.Id,
                                CategoryId = categoryId
                            });
                    }
                }

                var mainImageUrl = await FirebaseUtil.UploadFileToFirebase(request.MainImage, _configuration);
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
                    var imageUrls = await FirebaseUtil.UploadFilesToFirebase(request.SecondaryImages, _configuration);
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
            catch (Exception e)
            {
                return null;
            }

        }
    }
    

    public async Task<GetProductResponse> UpdateProductByIdAsync(Guid id, UpdateProductRequest request)
    {
        if(id == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        // var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     predicate: p => p.Id == id
        // );
        var product = await _productRepository.GetProductByIdAsync(id);
        if(product == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Quantity = request.Quantity;
        product.IsHidden = request.IsHidden;
        product.IsKit = request.IsKit;
        product.ModifiedAt = TimeUtil.GetCurrentSEATime();
        
        // _unitOfWork.GetRepository<Product>().UpdateAsync(product);
        _productRepository.UpdateAsync(product);
        bool isSuccess = await _productRepository.SaveChangesWithTransactionAsync();
        GetProductResponse productResponse = null;
        if (isSuccess) productResponse = _mapper.Map<GetProductResponse>(product);
        return productResponse;
    }

    public async Task<GetProductResponse> UpdateProductRelationshipByProductIdAsync(Guid parentId, UpdateChildProductForKitRequest request)
    {
        if(parentId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ProductIdNotNull);
        if (!request.ChildProductIds.Any())
            throw new BadHttpRequestException(MessageConstant.Product.ChildProductIdNotNull);
        // var parentProduct = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //     predicate: p => p.Id == parentId,
        //     include: p => p.Include(p => p.ChildProducts)
        //         .ThenInclude(pr => pr.ChildProduct)
        // );
        var parentProduct = await _productRepository.GetProductByIdAsync(parentId);
        
        if(parentProduct == null) throw new BadHttpRequestException(MessageConstant.Product.ProductNotFound);

        var currentChildProductIds = parentProduct.ChildProducts!.Select(pr => pr.ChildProductId).ToList();
        var addChildProductIds = request.ChildProductIds.Except(currentChildProductIds);
        var removeChildProductIds = currentChildProductIds.Except(request.ChildProductIds);

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
                        var removeChildProduct = parentProduct.ChildProducts
                            .FirstOrDefault(pr => pr.ChildProductId == removeChildProductId);
                        _productRelationshipRepository.DeleteAsync(removeChildProduct);
                    }
                }
                if (addChildProductIds.Any())
                {
                    foreach (var addChildProductId in addChildProductIds)
                    {
                        var addChildProduct = await _productRepository.GetProductByIdAsync(addChildProductId);
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
            catch (Exception e)
            {
                return null;
            }
        }
    }

    public async Task<ICollection<GetProductResponse>> GetChildProductsByParentIdAsync(Guid parentId)
    {
        if(parentId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Product.ParentProductIdNotNull);
        // var childProducts = await _unitOfWork.GetRepository<Product>().GetListAsync(
        //     predicate: p => p.ChildProducts.Any(cp => cp.ParentProductId == parentId)
        // );
        var childProducts = await _productRepository.GetListProductsByParentIdAsync(parentId);
        var childProductResponses = _mapper.Map<ICollection<GetProductResponse>>(childProducts);
        return childProductResponses;
    }

    public async Task<IPaginate<GetProductResponse>> GetProductByCategoryIdAsync(Guid categoryId, int page, int size)
    {
        if (categoryId == Guid.Empty) throw new BadHttpRequestException(MessageConstant.Category.CategoryIdNotNull);
        // var products = await _unitOfWork.GetRepository<Product>().GetPagingListAsync(
        //     selector: p => new GetProductResponse()
        //     {
        //         Id = p.Id,
        //         Name = p.Name,
        //         Description = p.Description,
        //         Quantity = p.Quantity,
        //         Price = p.Price,
        //         IsHidden = p.IsHidden,
        //         IsKit = p.IsKit,
        //         CreatedAt = p.CreatedAt,
        //         ModifiedAt = p.ModifiedAt,
        //         ProductImages = p.ProductImages.Select(pi => new ProductImageResponse()
        //         {
        //             Id = pi.Id,
        //             ImageUrl = pi.ImageUrl,
        //             isMain = pi.isMain
        //         }).ToList(),
        //     },
        //     predicate: p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId),
        //     page: page,
        //     size: size,
        //     include: p => p.Include(p => p.ProductImages)
        // );
        var products = await _productRepository.GetProductsPagingByCategoryId(categoryId, page, size);
        var productResponses = _mapper.Map<IPaginate<GetProductResponse>>(products);
        return productResponses;
    }
}