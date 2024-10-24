using KALS.Domain.Entities;

namespace KALS.Repository.Interface;

public interface IProductImageRepository: IGenericRepository<ProductImage>
{
    Task<ProductImage> GetProductImageByIdAsync(Guid id);

    Task<ICollection<ProductImage>> GetProductImagesByProductId(Guid productId);
    
    
}