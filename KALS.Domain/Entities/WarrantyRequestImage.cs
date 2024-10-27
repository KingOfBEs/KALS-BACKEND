using System.ComponentModel.DataAnnotations.Schema;
using KALS.Domain.Common;

namespace KALS.Domain.Entities;

public class WarrantyRequestImage: BaseEntity
{
    public string ImageUrl { get; set; }
    public Guid WarrantyRequestId { get; set; }
    [ForeignKey(nameof(WarrantyRequestId))]
    public WarrantyRequest WarrantyRequest { get; set; }
}