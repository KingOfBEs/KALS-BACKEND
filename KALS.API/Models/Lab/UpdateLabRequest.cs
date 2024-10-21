namespace KALS.API.Models.Lab;

public class UpdateLabRequest
{
    public string? Name { get; set; }
    public IFormFile? File { get; set; }
}