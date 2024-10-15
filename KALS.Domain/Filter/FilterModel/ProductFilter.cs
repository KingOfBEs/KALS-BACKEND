using System.Linq.Expressions;
using KALS.Domain.Entities;
using KALS.Domain.Filter;

namespace KALS.Domain.Filter.FilterModel;

public class ProductFilter: IFilter<Product>
{
    public string? Name { get; set; }
    public DateTime? CreateAt { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public bool? IsKit { get; set; }
    
    public Expression<Func<Product, bool>> ToExpression()
    {
        return product => 
            (string.IsNullOrEmpty(Name) || product.Name.Contains(Name)) &&
            (!CreateAt.HasValue || product.CreatedAt == CreateAt) &&
            (CategoryIds == null || product.ProductCategories!.Any(pc => CategoryIds.Contains(pc.CategoryId))) &&
            (!IsKit.HasValue || product.IsKit == IsKit);
            
    }
}