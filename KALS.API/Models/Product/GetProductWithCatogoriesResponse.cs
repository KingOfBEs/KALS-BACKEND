using KALS.API.Models.Category;

namespace KALS.API.Models.Product;

public class GetProductWithCatogoriesResponse
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
    public ICollection<CategoryResponse> Categories { get; set; }
}