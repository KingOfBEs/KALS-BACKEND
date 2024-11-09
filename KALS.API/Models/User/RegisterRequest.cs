using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.User;

public class RegisterRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [MaxLength(20)]
    public string Password { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Otp { get; set; }
}