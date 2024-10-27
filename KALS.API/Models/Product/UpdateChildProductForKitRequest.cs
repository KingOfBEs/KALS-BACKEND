using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Product;

public class UpdateChildProductForKitRequest
{
    [Required]
    public List<Guid> ChildProductIds { get; set; }
}