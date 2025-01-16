using Microsoft.EntityFrameworkCore;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Database;

public class TarjetaDbContext(DbContextOptions<TarjetaDbContext> options) : DbContext(options)
{
    public DbSet<TarjetaEntity> Tarjetas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TarjetaEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
        });
    }
}