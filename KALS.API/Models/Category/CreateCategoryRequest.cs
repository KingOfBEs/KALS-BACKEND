using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Category;

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
}