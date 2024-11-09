using System.Linq.Expressions;
using System.Reflection;
using KALS.Domain.DataAccess;
using KALS.Domain.Filter;
using KALS.Domain.Paginate;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Tokens;

namespace KALS.Repository.Implement;

public class GenericRepository<T>: IGenericRepository<T>, IAsyncDisposable where T : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _dbSet;
    
    public GenericRepository(DbContext context)
    {
        _dbContext = context;
        _dbSet = context.Set<T>();
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
    
    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    #region Get
    public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        IQueryable<T> query = _dbSet;
        if (include != null) query = include(query);
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return orderBy(query).AsNoTracking().FirstOrDefaultAsync();
        
        return query.AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        IQueryable<T> query = _dbSet;
        if (predicate != null) query = query.Where(predicate);
        if (include != null) query = include(query);
        if (orderBy != null) return orderBy(query).AsNoTracking().Select(selector).FirstOrDefaultAsync();
        
        return query.AsNoTracking().Select(selector).FirstOrDefaultAsync();
    }
    
    public async Task<IPaginate<TResult>> GetPagingListAsync<TResult>(Expression<Func<T, TResult>> selector, IFilter<T> filter, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int page = 1, int size = 10, string sortBy = null, bool isAsc = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (filter != null)
        {
            var filterExpression = filter.ToExpression();
            query = query.Where(filterExpression);
        }
        if (predicate != null) query = query.Where(predicate);
        if (include != null) query = include(query);
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = ApplySort(query, sortBy, isAsc);
        }
        else if (orderBy != null)
        {
            query = orderBy(query);
        }
        
        return await query.AsNoTracking().Select(selector).ToPaginateAsync(page, size, 1);
      
    }
    
    private IQueryable<T> ApplySort(IQueryable<T> query, string sortBy, bool isAsc)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            throw new ArgumentException($"Property '{sortBy}' not found on type {typeof(T).Name}");
        }
        var propertyAccess = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);
         
        string methodName = isAsc ? "OrderBy" : "OrderByDescending";

        var resultExpression = Expression.Call(typeof(Queryable), methodName, 
                new Type[] {typeof(T), propertyAccess.Type},
                query.Expression, Expression.Quote(lambda));
        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public async Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
    {
        
        IQueryable<T> query = _dbSet;
        if (include != null) query = include(query);
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return await orderBy(query).AsNoTracking().ToListAsync();

        return await query.AsNoTracking().ToListAsync();
    }
    #endregion

    #region Insert
    public async Task InsertAsync(T entity)
    {
        if (entity == null) return;
        await _dbSet.AddAsync(entity);
    }

    public async Task InsertRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null) return;
        _dbSet.AddRangeAsync(entities);
    }
    #endregion

    #region Update
    public void UpdateAsync(T entity)
    {
        if(entity == null) return;
        _dbSet.Update(entity);
    }

    public void UpdateRangeAsync(IEnumerable<T> entities)
    {
        if(entities == null) return;
        _dbSet.UpdateRange(entities);
    }

    public void DeleteAsync(T entity)
    {
        if(entity == null) return;
        _dbSet.Remove(entity);
    }

    #endregion
    public async Task<bool> SaveChangesWithTransactionAsync()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await _dbContext.SaveChangesAsync() > 0;
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        var result = await _dbContext.SaveChangesAsync() > 0;
        return result;
    }
}