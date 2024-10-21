using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Product;

public class AddImageProductRequest
{
    [Required]
    public IFormFile Image { get; set; }
    [Required]
    public bool IsMain { get; set; }
}