using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Lab;

public class AssignLabsToProductRequest
{
    [Required]
    public List<Guid> LabIds { get; set; }
}