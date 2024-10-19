namespace KALS.API.Models.User;

public class MemberResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string PhoneNumber { get; set; }
    public string FullName { get; set; }
    public string? Commune { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public string? Address { get; set; }
    public string? ProvinceCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? CommuneCode { get; set; }
}