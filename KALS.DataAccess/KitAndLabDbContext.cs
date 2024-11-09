using System.Reflection;
using KALS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KALS.Domain.DataAccess;

public class KitAndLabDbContext : DbContext
{
    public KitAndLabDbContext()
    {
    }
    public KitAndLabDbContext(DbContextOptions<KitAndLabDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> User { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<ProductCategory> ProductCategory { get; set; }
    public DbSet<Lab> Lab { get; set; }
    public DbSet<Member> Member { get; set; }
    public DbSet<Staff> Staff { get; set; } 
    public DbSet<Payment> Payment { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
    public DbSet<SupportRequest> SupportRequest { get; set; }
    public DbSet<SupportMessage> SupportMessage { get; set; }
    public DbSet<ProductImage> ProductImage { get; set; }
    public DbSet<SupportMessageImage> SupportMessageImage { get; set; }
    public DbSet<WarrantyRequest> WarrantyRequest { get; set; }
    public DbSet<WarrantyRequestImage> WarrantyRequestImage { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");
        modelBuilder.Entity<Lab>().ToTable("Lab");
        modelBuilder.Entity<Member>().ToTable("Member");
        modelBuilder.Entity<Staff>().ToTable("Staff");
        modelBuilder.Entity<Payment>().ToTable("Payment");
        modelBuilder.Entity<Order>().ToTable("Order");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItem");
        modelBuilder.Entity<SupportRequest>().ToTable("SupportRequest");
        modelBuilder.Entity<SupportMessage>().ToTable("SupportMessage");
        modelBuilder.Entity<ProductImage>().ToTable("ProductImage");
        modelBuilder.Entity<SupportMessageImage>().ToTable("SupportMessageImage");
        modelBuilder.Entity<WarrantyRequest>().ToTable("WarrantyRequest");
        modelBuilder.Entity<WarrantyRequestImage>().ToTable("WarrantyRequestImage");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=KALS;User Id=sa;Password=123456aA@$;Encrypt=True;TrustServerCertificate=True");
            // optionsBuilder.UseSqlServer(
            // "Server=103.238.235.227,1433;Database=KALS;User Id=sa;Password=$Thanhkhoa;Encrypt=True;TrustServerCertificate=True"
            // );
            optionsBuilder.UseSqlServer(
                "Server=103.238.235.227,1433;Database=KALS-Production;User Id=sa;Password=$Thanhkhoa;Encrypt=True;TrustServerCertificate=True"
            );
        }
    }
 }