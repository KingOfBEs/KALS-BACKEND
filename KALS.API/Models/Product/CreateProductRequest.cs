using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Product;

public class CreateProductRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public bool IsKit { get; set; }
    [Required]
    public bool IsHidden { get; set; }
    public List<Guid>? ChildProductIds { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    [Required]
    public string MainImage { get; set; }
    public List<string>? SecondaryImages { get; set; }
    
}