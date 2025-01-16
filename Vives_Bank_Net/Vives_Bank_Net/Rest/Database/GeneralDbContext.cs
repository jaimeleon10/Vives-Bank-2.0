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
    }
}