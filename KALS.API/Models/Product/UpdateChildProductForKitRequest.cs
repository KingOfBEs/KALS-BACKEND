using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Product;

public class UpdateChildProductForKitRequest
{
    [Required]
    public Guid ChildProductId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lơn hơn 0")]
    public int Quantity { get; set; }
}