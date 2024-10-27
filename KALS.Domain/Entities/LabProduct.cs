using System.ComponentModel.DataAnnotations.Schema;

namespace KALS.Domain.Entities;

public class LabProduct
{
    public Guid LabId { get; set; }
    [ForeignKey(nameof(LabId))]
    public Lab Lab { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    
}