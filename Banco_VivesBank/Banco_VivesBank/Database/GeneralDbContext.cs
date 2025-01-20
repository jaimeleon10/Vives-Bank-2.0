using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Database;

public class GeneralDbContext : DbContext
{
    public GeneralDbContext(DbContextOptions<GeneralDbContext> options) : base(options) {}
    
    public DbSet<UserEntity> Usuarios { get; set; }
    public DbSet<ClienteEntity> Clientes { get; set; }
    public DbSet<BaseEntity> ProductoBase { get; set; }
    public DbSet<CuentaEntity> Cuentas { get; set; }
    public DbSet<TarjetaEntity> Tarjetas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Ignore<User.Models.User>();
        modelBuilder.Ignore<Cliente.Models.Cliente>();
        modelBuilder.Ignore<BaseModel>();
        modelBuilder.Ignore<Cuenta>();
        modelBuilder.Ignore<TarjetaModel>();
        
        /*
        // User Entity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAddOrUpdate();
        });
        */

        // Cliente Entity
        modelBuilder.Entity<ClienteEntity>(entity =>
        {
            entity.Property(ent => ent.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(ent => ent.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
            entity.OwnsOne(e => e.Direccion, direccion =>
            {
                direccion.Property(d => d.Calle).HasColumnName("Calle");
                direccion.Property(d => d.Numero).HasColumnName("Numero");
                direccion.Property(d => d.CodigoPostal).HasColumnName("CodigoPostal");
                direccion.Property(d => d.Piso).HasColumnName("Piso");
                direccion.Property(d => d.Letra).HasColumnName("Letra");
            });
        });
        /*
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
        #1#

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
        });*/
    }
}