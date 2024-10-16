namespace KALS.API.Models.SupportRequest;

public class SupportRequest
{
    public string Content { get; set; }
    public Guid LabId { get; set; }
    
    public List<IFormFile> ImageFiles { get; set; }
}