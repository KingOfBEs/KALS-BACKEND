using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;
using KALS.Domain.Entities;

namespace KALS.Domain.Entities;

public class Product: BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsHidden { get; set; }
    public bool IsKit { get; set; }
    
    
    public virtual ICollection<ProductRelationship>? ParentProducts { get; set; } 
    public virtual ICollection<ProductRelationship>? ChildProducts { get; set; } 
    public virtual ICollection<Lab>? Labs { get; set; }
    public virtual ICollection<ProductCategory>? ProductCategories { get; set; }
    public virtual ICollection<ProductImage>? ProductImages { get; set; }
}