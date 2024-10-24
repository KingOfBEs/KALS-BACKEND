using System.Linq.Expressions;
using KALS.Domain.Entities;

namespace KALS.Domain.Filter.FilterModel;

public class CategoryFilter: IFilter<Category>
{
    public string? Name { get; set; }
    public Expression<Func<Category, bool>> ToExpression()
    {
        return category => 
            (string.IsNullOrEmpty(Name) || category.Name.Contains(Name));
     
    }
}