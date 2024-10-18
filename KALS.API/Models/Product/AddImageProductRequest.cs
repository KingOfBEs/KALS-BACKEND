namespace KALS.API.Models.Product;

public class AddImageProductRequest
{
    public IFormFile Image { get; set; }
    public bool IsMain { get; set; }
}