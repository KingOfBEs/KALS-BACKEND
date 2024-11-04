using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Lab;

public class CreateLabRequest
{
    public string? Name { get; set; }
    [Required]
    public string VideoUrl { get; set; }
    [Required]
    public IFormFile File { get; set; }
    [Required]
    public Guid ProductId { get; set; }
}