using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Payment;

public class UpdatePaymentOrderStatusRequest
{
    [Required]
    public int OrderCode { get; set; }
}