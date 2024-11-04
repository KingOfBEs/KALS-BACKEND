namespace KALS.API.Models.Lab;

public class UpdateLabRequest
{
    public string? Name { get; set; }
    public string? VideoUrl { get; set; }
    public IFormFile? File { get; set; }
    
    public Guid? ProductId { get; set; }
}