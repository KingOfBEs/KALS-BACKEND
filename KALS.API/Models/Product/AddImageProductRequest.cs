using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Product;

public class AddImageProductRequest
{
    public Guid? Id { get; set; }
    [Required]
    public string ImageUrl { get; set; }
    [Required]
    public bool IsMain { get; set; }
}