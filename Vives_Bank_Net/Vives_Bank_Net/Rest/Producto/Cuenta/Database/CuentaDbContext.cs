using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Vives_Banks_Net.Rest.Cliente;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Database;

public class CuentaDbContext(DbContextOptions<CuentaDbContext> options) : DbContext(options)
{
    public DbSet<CuentaEntity> Cuentas { get; set; }
    //public DbSet<Tarjeta> Tarjetas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    //public DbSet<Producto> Productos { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentaEntity>()
            .HasKey(c => c.Id); 

        modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.Cliente) 
            .WithMany() 
            .HasForeignKey(c => c.ClienteId) 
            .IsRequired(); 
        /*
        modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.Tarjeta) 
            .WithOne() 
            .HasForeignKey(c => c.TarjetaId)
            .IsRequired(false);

        modelBuilder.Entity<CuentaEntity>()
            .HasOne(c => c.Producto) 
            .WithMany() 
            .HasForeignKey(c => c.ProductoId)
            .IsRequired(); 
        */
        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.Guid)
            .IsRequired(); 
        
        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.Iban)
            .IsRequired(); 

        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.Saldo)
            .IsRequired(); 

        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.IsDeleted)
            .IsRequired();

        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.CreatedAt)
            .IsRequired();

        modelBuilder.Entity<CuentaEntity>()
            .Property(c => c.UpdatedAt)
            .IsRequired();
    }
}