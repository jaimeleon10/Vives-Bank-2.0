using Microsoft.EntityFrameworkCore;
using Vives_Bank_Net.Rest.Cliente.Database;
using Vives_Bank_Net.Rest.Producto.Base.Database;
using Vives_Bank_Net.Rest.Producto.Base.Models;
using Vives_Bank_Net.Rest.Producto.Cuenta.Database;
using Vives_Bank_Net.Rest.Producto.Cuenta.Models;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Database;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Models;
using Vives_Bank_Net.Rest.User.Database;

namespace Vives_Bank_Net.Rest.Database;

public class GeneralDbContext : DbContext
{
    public GeneralDbContext(DbContextOptions<GeneralDbContext> options) : base(options) {}
    
    public DbSet<UserEntity> Usuarios { get; set; }
    public DbSet<ClienteEntity> Clientes { get; set; }
    public DbSet<BaseEntity> Bases { get; set; }
    public DbSet<CuentaEntity> Cuentas { get; set; }
    public DbSet<TarjetaEntity> Tarjetas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>().ToTable("Usuarios");
        modelBuilder.Entity<ClienteEntity>().ToTable("Clientes");
        modelBuilder.Entity<BaseEntity>().ToTable("ProductosBase");
        modelBuilder.Entity<CuentaEntity>().ToTable("Cuentas");
        modelBuilder.Entity<TarjetaEntity>().ToTable("Tarjetas");

        // User Entity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAddOrUpdate(); // Asegura la actualización
        });

        // Cliente Entity
        modelBuilder.Entity<ClienteEntity>(entity =>
        {
            entity.Property(ent => ent.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(ent => ent.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
            entity.OwnsOne(e => e.Direccion);
            // TODO: Revisar relacidón Cliente - Usuario
        });
        
        // Base Entity
        modelBuilder.Entity<BaseEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
            // TODO: Revisar si hay que añadir relación
        });
        
        // Cuenta Entity
        // TODO: Revisar todas las relaciones
        modelBuilder.Entity<CuentaEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
        });
        
        modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.Cliente) 
            .WithMany() 
            .HasForeignKey(c => c.ClienteId) 
            .IsRequired(); 
        
        /*
         modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.TarjetaId)
            .WithOne()
            .HasForeignKey(c => c.TarjetaId)
            .IsRequired(false);
        */
        
        modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.Producto) 
            .WithMany() 
            .HasForeignKey(c => c.ProductoId)
            .IsRequired(); 
        
        // Tarjeta Entity
        modelBuilder.Entity<TarjetaEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
        });
    }
}