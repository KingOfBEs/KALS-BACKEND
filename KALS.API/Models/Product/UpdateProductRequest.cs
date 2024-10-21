namespace KALS.API.Models.Product;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public bool? IsHidden { get; set; }
    public bool? IsKit { get; set; }
    
}