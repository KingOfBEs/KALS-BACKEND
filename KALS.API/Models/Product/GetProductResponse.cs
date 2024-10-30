using KALS.Domain.Entities;

namespace KALS.API.Models.Product;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public bool IsKit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsHidden { get; set; }
    public ICollection<ProductImageResponse>? ProductImages { get; set; } 
    
}