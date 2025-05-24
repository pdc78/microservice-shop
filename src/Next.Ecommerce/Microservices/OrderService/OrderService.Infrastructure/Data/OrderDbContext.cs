using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<OrderItem>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade); 

        base.OnModelCreating(modelBuilder);
    }
}