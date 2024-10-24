using KALS.Domain.Entities;
using KALS.Domain.Filter.FilterModel;
using KALS.Domain.Paginate;

namespace KALS.Repository.Interface;

public interface ICategoryRepository: IGenericRepository<Category>
{
    Task<IPaginate<Category>> GetCategoriesPaginateAsync(int page, int size, CategoryFilter? filter);
    Task<Category> GetCategoryByIdAsync(Guid id);
}