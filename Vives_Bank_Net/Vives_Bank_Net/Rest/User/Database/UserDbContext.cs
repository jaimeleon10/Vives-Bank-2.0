/*using Microsoft.EntityFrameworkCore;
using Vives_Bank_Net.Rest.User.Database;

public class UserDbContext : DbContext // Corrección de la definición del constructor
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) // Ajuste en la base class invocation
    {
    }

    public DbSet<UserEntity> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).IsRequired().ValueGeneratedOnAddOrUpdate(); // Asegura la actualización
        });
    }
}*/