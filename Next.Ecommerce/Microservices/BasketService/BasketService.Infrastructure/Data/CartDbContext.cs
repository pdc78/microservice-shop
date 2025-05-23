namespace CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BasketService.Domain.Entities;

public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options) { }

    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<BasketItem>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<BasketItem>()
            .HasOne(i => i.Basket)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}