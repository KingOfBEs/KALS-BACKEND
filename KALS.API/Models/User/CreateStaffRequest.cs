using System.ComponentModel.DataAnnotations;
using KALS.Domain.Enums;

namespace KALS.API.Models.User;

public class CreateStaffRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public StaffType Type { get; set; }
}