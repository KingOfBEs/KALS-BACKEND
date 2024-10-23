using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.Cart;

public class CheckOutRequest
{
    [Required]
    public string Address { get; set; }
    
}