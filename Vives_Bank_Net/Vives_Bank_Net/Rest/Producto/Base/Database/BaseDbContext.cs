/*using Microsoft.EntityFrameworkCore;

namespace Vives_Bank_Net.Rest.Producto.Base.Database;

public class BaseDbContext(DbContextOptions<BaseDbContext> options) : DbContext(options)
{
    public DbSet<BaseEntity> Bases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
        });
    }
}*/