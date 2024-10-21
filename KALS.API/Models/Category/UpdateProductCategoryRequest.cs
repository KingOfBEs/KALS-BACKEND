using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Category;

public class UpdateProductCategoryRequest
{
    [Required]
    public List<Guid> ProductIds { get; set; }
}