namespace KALS.API.Models.WarrantyRequest;

public class WarrantyRequestWithImageResponse
{
    public Guid Id { get; set; }
    public string RequestContent { get; set; }
    public string? ResponseContent { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid? ResponseBy { get; set; }
    public Guid OrderItemId { get; set; }
    
    public ICollection<WarrantyRequestImageResponse> WarrantyRequestImages { get; set; }
}