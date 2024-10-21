using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.User;

public class LoginRequest
{
    [Required]
    public string UsernameOrPhoneNumber { get; set; }
    [Required]
    public string Password { get; set; }
}