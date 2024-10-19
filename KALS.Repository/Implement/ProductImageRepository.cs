using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class ProductImageRepository: GenericRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(DbContext context) : base(context)
    {
    }

    public async Task<ProductImage> GetProductImageByIdAsync(Guid id)
    {
        var productImage = await SingleOrDefaultAsync(
            predicate: pi => pi.Id == id,
            include: pi => pi.Include(pi => pi.Product)
        );
        return productImage;
    }

    public async Task<ICollection<ProductImage>> GetProductImagesByProductId(Guid productId)
    {
        var productImages = await GetListAsync(
            predicate: pi => pi.ProductId == productId,
            include: pi => pi.Include(pi => pi.Product)
        );
        return productImages;
    }
}