using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Category;

public class UpdateCategoryRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
}