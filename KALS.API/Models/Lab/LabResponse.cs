using KALS.API.Models.Product;

namespace KALS.API.Models.Lab;

public class LabResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
    
    public int? NumberOfRequest { get; set; }
    public GetProductResponse Product { get; set; }
}