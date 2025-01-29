using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.Utils.Generators;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Database;

public class GeneralDbContext : DbContext
{
    public GeneralDbContext(DbContextOptions<GeneralDbContext> options) : base(options) {}
    
    public DbSet<UserEntity> Usuarios { get; set; }
    public DbSet<ClienteEntity> Clientes { get; set; }
    public DbSet<ProductoEntity> ProductoBase { get; set; }
    public DbSet<CuentaEntity> Cuentas { get; set; }
    public DbSet<TarjetaEntity> Tarjetas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User Entity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAdd();
            
            // Datos por defecto de usuario
            entity.HasData(new UserEntity
            {
                Id = 1,
                Guid = "vz2AWLK8YPS",
                Username = "pedrito",
                Password = "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe",
                Role = Role.Cliente,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new UserEntity
            {
                Id = 2,
                Guid = "6t8gVeTQt2w",
                Username = "anita",
                Password = "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe",
                Role = Role.Cliente,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new UserEntity
            {
                Id = 3,
                Guid = "u6b6NDClz5o",
                Username = "admin",
                Password = "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe",
                Role = Role.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new UserEntity
            {
                Id = 4,
                Guid = "03IBwamDHa5",
                Username = "user",
                Password = "$2a$11$H8eSJTQ0cZjHNmozhjcW6ep/5jUQDnt7FrUmgbNKxww897iMniVfe",
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        });

        // Cliente Entity
        modelBuilder.Entity<ClienteEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAdd();
            
            // Campo Dirección en Cliente
            entity.OwnsOne(e => e.Direccion, dir =>
            {
                dir.Property(d => d.Calle).HasColumnName("Calle");
                dir.Property(d => d.Numero).HasColumnName("Numero");
                dir.Property(d => d.CodigoPostal).HasColumnName("CodigoPostal");
                dir.Property(d => d.Piso).HasColumnName("Piso");
                dir.Property(d => d.Letra).HasColumnName("Letra");
            });
            
            // Relación Cliente-User (1:1)
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<ClienteEntity>(e => e.UserId)
                .IsRequired();
            
            // Datos por defecto de cliente
            entity.HasData(new ClienteEntity
            {
                Id = 1,
                Guid = "GbJtJkggUOM",
                Dni = "12345678Z",
                Nombre = "Pedro",
                Apellidos = "Picapiedra",
                Email = "pedro.picapiedra@gmail.com",
                Telefono = "612345678",
                FotoPerfil = "https://example.com/fotoPerfilPedro.jpg",
                FotoDni = "https://example.com/fotoDniPedro.jpg",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new ClienteEntity
            {
                Id = 2,
                Guid = "JdHsgzoHlrb",
                Dni = "21240915R",
                Nombre = "Ana",
                Apellidos = "Martinez",
                Email = "ana.martinez@gmail.com",
                Telefono = "623456789",
                FotoPerfil = "https://example.com/fotoPerfilAna.jpg",
                FotoDni = "https://example.com/fotoDniAna.jpg",
                UserId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            // Datos por defecto de direccion para cada cliente
            entity.OwnsOne(e => e.Direccion).HasData(new
            {
                ClienteEntityId = 1L,
                Calle = "Calle Uno",
                Numero = "123",
                CodigoPostal = "28001",
                Piso = "1",
                Letra = "A"
            });
            
            entity.OwnsOne(e => e.Direccion).HasData(new
            {
                ClienteEntityId = 2L,
                Calle = "Calle Dos",
                Numero = "456",
                CodigoPostal = "28002",
                Piso = "2",
                Letra = "B"
            });
        });
        
        // Base Entity
        modelBuilder.Entity<ProductoEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAdd();
            
            // Datos por defecto de producto
            entity.HasData(new ProductoEntity
            {
                Id = 1,
                Guid = "yFlOirSXTaL",
                Nombre = "Cuenta de ahorros",
                Descripcion = "Producto para cuenta bancaria de ahorros",
                TipoProducto = "cuentaAhorros",
                Tae = 2.5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new ProductoEntity
            {
                Id = 2,
                Guid = "dEmAjXpMTmy",
                Nombre = "Cuenta corriente",
                Descripcion = "Producto para cuenta bancaria corriente",
                TipoProducto = "cuentaCorriente",
                Tae = 1.5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        });
        
        // Cuenta Entity
        modelBuilder.Entity<CuentaEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAdd();
            
            // Relación Cliente-Cuenta (1:N)
            entity.HasOne(c => c.Cliente)
                .WithMany(c => c.Cuentas)
                .HasForeignKey(c => c.ClienteId)
                .IsRequired();
            
            // Relación Cuenta-Producto (1:1)
            entity.HasOne(c => c.Producto)
                .WithMany()
                .HasForeignKey(c => c.ProductoId)
                .IsRequired();
            
            // Relación Cuenta-Tarjeta (1:0..1)
            entity.HasOne(c => c.Tarjeta)
                .WithOne()
                .HasForeignKey<CuentaEntity>(c => c.TarjetaId);
            
            // Datos por defecto de cuenta
            entity.HasData(new CuentaEntity
            {
                Id = 1,
                Guid = "VWt47641yDI",
                Iban = "ES7730046576085345979538",
                Saldo = 5000,
                TarjetaId = 1,
                ClienteId = 1,
                ProductoId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new CuentaEntity
            {
                Id = 2,
                Guid = "oVUzRuFwMlf",
                Iban = "ES2114656261103572788444",
                Saldo = 7000,
                TarjetaId = 2,
                ClienteId = 2,
                ProductoId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        });
        
        // Tarjeta Entity
        modelBuilder.Entity<TarjetaEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAdd();
            
            // Datos por defecto
            entity.HasData(new TarjetaEntity
            {
                Id = 1,
                Guid = "HGyMfulgniP",
                Numero = "0606579225434779",
                FechaVencimiento = "04/27",
                Cvv = "298",
                Pin = "1234",
                LimiteDiario = 500,
                LimiteSemanal = 2500,
                LimiteMensual = 10000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            
            entity.HasData(new TarjetaEntity
            {
                Id = 2,
                Guid = "W71vOHuFzS4",
                Numero = "0751528101703123",
                FechaVencimiento = "06/26",
                Cvv = "425",
                Pin = "4321",
                LimiteDiario = 100,
                LimiteSemanal = 1500,
                LimiteMensual = 2500,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        });
    }
}