namespace KALS.API.Models.SupportRequest;

public class ResponseSupportRequest
{
    public string? Content { get; set; }
    
    public List<IFormFile>? ImageFiles { get; set; }
}