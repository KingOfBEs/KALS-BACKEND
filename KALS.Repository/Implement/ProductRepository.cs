using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class ProductRepository: GenericRepository<Product>, IProductRepository
{
    public ProductRepository(KitAndLabDbContext context) : base(context)
    {
    }

    public Task<Product> GetProductByIdAsync(Guid id)
    {

        var product = SingleOrDefaultAsync(
            predicate:x => x.Id == id,
            include: p => p.Include(p => p.ChildProducts)
                .ThenInclude(cp => cp.ChildProduct)
                .Include(p => p.ProductImages)
                .Include(p => p.LabProducts)
                .ThenInclude(p => p.Lab)
        );
        return product;
    }

    public async Task<IPaginate<Product>> GetProductPagingAsync(int page, int size, IFilter<Product> filter,
        string? sortBy, bool isAsc)
    {
        var products = await GetPagingListAsync(
            selector: p => new Product()
            {
                Id = p.Id,
                Description = p.Description,
                Quantity = p.Quantity,
                Name = p.Name,
                Price = p.Price,
                IsHidden = p.IsHidden,
                IsKit = p.IsKit,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt,
                ProductCategories = p.ProductCategories.Any(pc => pc.ProductId == p.Id) ? p.ProductCategories : null,
                ProductImages = p.ProductImages.Any(pi => pi.ProductId == p.Id) ? p.ProductImages : null,
                ChildProducts = p.ChildProducts.Any(cp => cp.ParentProductId == p.Id) ? p.ChildProducts : null
            },
            predicate: p => !p.IsHidden,
            page: page,
            size: size,
            include: p => p.Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category),
            filter: filter,
            sortBy: sortBy,
            isAsc: isAsc
        );
        return products;
    }

    public async Task<ICollection<Product>> GetListProductsByParentIdAsync(Guid parentId)
    {
        var products = await GetListAsync(
            predicate: p => p.ChildProducts.Any(cp => cp.ParentProductId == parentId)
        );
        return products;
    }

    public async Task<IPaginate<Product>> GetProductsPagingByCategoryId(Guid categoryId, int page, int size)
    {
        var products = await GetPagingListAsync(
            selector: p => new Product()
            {
                Id = p.Id,
                Description = p.Description,
                Quantity = p.Quantity,
                Name = p.Name,
                Price = p.Price,
                IsHidden = p.IsHidden,
                IsKit = p.IsKit,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt,
                ProductCategories = p.ProductCategories.Any(pc => pc.ProductId == p.Id) ? p.ProductCategories : null,
                ProductImages = p.ProductImages.Any(pi => pi.ProductId == p.Id) ? p.ProductImages : null,
                LabProducts = p.LabProducts.Any(lp => lp.ProductId == p.Id) ? p.LabProducts : null,
                ChildProducts = p.ChildProducts.Any(cp => cp.ParentProductId == p.Id) ? p.ChildProducts : null
            },
            predicate: p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId),
            page: page,
            size: size,
            include: p => p.Include(p => p.ProductImages),
            filter: null
        );
        return products;
    }
}