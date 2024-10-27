using System.Security.Cryptography;
using System.Text;
using KALS.Domain.Entities;
using KALS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KALS.DataAccess.Persistent.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(r => r.Role)
            .HasConversion(
                v => v.ToString(),
                v => (RoleEnum)Enum.Parse(typeof(RoleEnum), v)
            );
        builder.HasData(new User()
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Password = Convert.ToBase64String(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes("admin"))),
            PhoneNumber = "0123456789",
            FullName = "Admin",
            Role = RoleEnum.Manager
        });
    }
}

public class PaymentConfiguration: IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v)
            );
    }
}
public class OrderConfiguration: IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Status)
            .HasConversion(
                v => v.ToString(),
                v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v)
            );
        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Order>(p => p.PaymentId);
    }
}

public class StaffConfiguration: IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.Property(s => s.Type)
            .HasConversion(
                s => s.ToString(),
                s => (StaffType)Enum.Parse(typeof(StaffType), s)
            );
    }
}
public class SupportMessageConfiguration: IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> builder)
    {
        builder.Property(sm => sm.Type)
            .HasConversion(
                smt => smt.ToString(),
                smt => (SupportMessageType)Enum.Parse(typeof(SupportMessageType), smt)
            );
    }
}
