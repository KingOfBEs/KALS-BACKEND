using System.Linq.Expressions;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;
using Microsoft.EntityFrameworkCore.Query;

namespace KALS.Repository.Interface;

public interface IGenericRepository<T>: IDisposable where T : class
{
    Task<T> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
    );
    Task<TResult> SingleOrDefaultAsync<TResult>
    (
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
    );
    Task<IPaginate<TResult>> GetPagingListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        IFilter<T> filter = null,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        int page = 1,
        int size = 10,
        string sortBy = null,
        bool isAsc = true
    );
    Task<ICollection<T>> GetListAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
    );
    
    Task InsertAsync(T entity);
    Task InsertRangeAsync(IEnumerable<T> entities);
    
    void UpdateAsync(T entity);
    void UpdateRangeAsync(IEnumerable<T> entities);
    
    void DeleteAsync(T entity);
    
    Task<bool> SaveChangesAsync();
    
    
}