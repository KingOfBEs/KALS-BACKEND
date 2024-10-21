using System.ComponentModel.DataAnnotations;

namespace KALS.API.Models.User;

public class GenerateOtpRequest
{
    [Required]
    public string PhoneNumber { get; set; }
}