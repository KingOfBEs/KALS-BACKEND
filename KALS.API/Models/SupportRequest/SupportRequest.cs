using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.SupportRequest;

public class SupportRequest
{
    [Required]
    public string Content { get; set; }
    [Required]
    public Guid LabId { get; set; }
    public List<IFormFile>? ImageFiles { get; set; }
}