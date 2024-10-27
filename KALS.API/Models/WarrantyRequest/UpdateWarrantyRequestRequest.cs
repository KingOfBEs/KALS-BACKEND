using System.ComponentModel.DataAnnotations;
using KALS.Domain.Enums;

namespace KALS.API.Models.WarrantyRequest;

public class UpdateWarrantyRequestRequest
{
    [Required]
    [MaxLength(1000)]
    public string ResponseContent { get; set; }
    [Required]
    public WarrantyRequestStatus Status { get; set; }
}