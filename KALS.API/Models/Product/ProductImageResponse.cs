namespace KALS.API.Models.Product;

public class ProductImageResponse
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public bool IsMain { get; set; }
}