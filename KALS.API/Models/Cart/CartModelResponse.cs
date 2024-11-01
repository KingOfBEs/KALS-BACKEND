namespace KALS.API.Models.Cart;

public class CartModelResponse
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? MainImage { get; set; }
    
    public int ProductQuantity { get; set; }
}