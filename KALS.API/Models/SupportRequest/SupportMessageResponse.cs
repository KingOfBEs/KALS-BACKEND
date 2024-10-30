using KALS.Domain.Enums;

namespace KALS.API.Models.SupportRequest;

public class SupportMessageResponse
{
    public Guid Id { get; set; }
    public SupportMessageType Type { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public ICollection<SupportMessageImageResponse> SupportMessageImages { get; set; }
}