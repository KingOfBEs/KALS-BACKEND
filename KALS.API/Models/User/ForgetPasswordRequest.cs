using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.User;

public class ForgetPasswordRequest
{
    [Required]
    public string Otp { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string Password { get; set; }
}