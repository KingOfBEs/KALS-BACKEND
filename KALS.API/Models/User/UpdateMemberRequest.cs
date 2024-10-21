namespace KALS.API.Models.User;

public class UpdateMemberRequest
{
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Commune { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? ProvinceCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? CommuneCode { get; set; }
    
    public void TrimString()
    {
        Username = Username?.Trim();
        FullName = FullName?.Trim();
        Commune = Commune?.Trim();
        Province = Province?.Trim();
        District = District?.Trim();
        Address = Address?.Trim();
        CommuneCode = CommuneCode?.Trim();
        DistrictCode = DistrictCode?.Trim();
        ProvinceCode = ProvinceCode?.Trim();
    }
}