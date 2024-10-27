using KALS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KALS.DataAccess.Persistent.Configuration;

public class LabProductConfiguration: IEntityTypeConfiguration<LabProduct>
{
    public void Configure(EntityTypeBuilder<LabProduct> builder)
    {
        builder.HasKey(lp => new { lp.LabId, lp.ProductId });

        builder
            .HasOne(lp => lp.Lab)
            .WithMany(l => l.LabProducts)
            .HasForeignKey(lp => lp.LabId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(lp => lp.Product)
            .WithMany(p => p.LabProducts)
            .HasForeignKey(lp => lp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}