using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class CategoryRepository: GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(DbContext context) : base(context)
    {
    }

    public async Task<IPaginate<Category>> GetCategoriesPaginateAsync(int page, int size)
    {
        var categories = await GetPagingListAsync(
            page: page,
            size: size,
            selector: c => new Category()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ModifiedAt = c.ModifiedAt
            },
            filter: null,
            orderBy: c => c.OrderByDescending(c => c.CreatedAt)
        );
        return categories;
    }

    public async Task<Category> GetCategoryByIdAsync(Guid id)
    {
        var category = await SingleOrDefaultAsync(
            predicate: c => c.Id == id, 
            include: c => c.Include(c => c.ProductCategories)
                .ThenInclude(pc => pc.Product)
        );
        return category;
    }
}