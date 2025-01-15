
namespace Vives_Bank_Net.Rest.Cliente.Database;

public class ClienteDbContext(DbContextOptions<ClienteDbContext> options): DbContext(options)
{
    public DbSet<ClienteEntity> Clientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClienteEntity>(entity =>
        {
            entity.Property(entity => entity.CreatedAt).IsRequired().ValueGeneratedOnAdd();
            entity.Property(entity => entity.UpdatedAt).IsRequired().ValueGeneratedOnUpdate();
        })
    }
}