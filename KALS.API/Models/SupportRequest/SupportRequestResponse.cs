using KALS.API.Models.User;
using KALS.Domain.Enums;

namespace KALS.API.Models.SupportRequest;

public class SupportRequestResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public SupportRequestStatus Status { get; set; }
    
    public ICollection<SupportMessageResponse>? SupportMessages { get; set; }
    
    public ICollection<string>? ImageUrls { get; set; }
    public MemberResponse Member { get; set; }
}