using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.WarrantyRequest;

public class CreateWarrantyRequestRequest
{
    [Required]
    public Guid OrderItemId { get; set; }
    [Required]
    [MaxLength(1000)]
    public string RequestContent { get; set; } 
    [Required]
    public ICollection<string> Images { get; set; }
}