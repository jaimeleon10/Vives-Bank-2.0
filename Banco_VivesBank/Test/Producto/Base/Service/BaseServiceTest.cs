using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace Test.Producto.Base.Service;

[TestFixture]
[TestOf(typeof(BaseService))]
public class BaseServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private BaseService _baseService;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();

        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new GeneralDbContext(options);

        await _dbContext.Database.EnsureCreatedAsync();

        await _dbContext.ProductoBase.ExecuteDeleteAsync();

        _baseService = new BaseService(_dbContext, NullLogger<BaseService>.Instance);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }
    
    [Test]
    public async Task GetAllAsync()
    {
        await _dbContext.ProductoBase.ExecuteDeleteAsync();

        for (int i = 0; i < 5; i++)
        {
            var baseEntity = new BaseEntity()
            {
                Guid = Guid.NewGuid().ToString(),
                Nombre = $"Producto {i + 1}",
                TipoProducto = $"Tipo {i + 1}"
            };
            _dbContext.ProductoBase.Add(baseEntity);
        }
        await _dbContext.SaveChangesAsync();

        var result = await _baseService.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(5));
    }
    
    [Test]
    public async Task GetByGuidAsync()
    {
        var guid = Guid.NewGuid().ToString();
        var baseEntity = new BaseEntity { Guid = guid, Nombre = "Producto 1", TipoProducto = "Tipo 1" };
        _dbContext.ProductoBase.Add(baseEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _baseService.GetByGuidAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That("Producto 1", Is.EqualTo(result?.Nombre));
    }

    [Test]
    public async Task GetByGuidAsyncProductNotExist()
    {
        var ex = Assert.ThrowsAsync<BaseNotExistException>(async () =>
        {
            await _baseService.GetByGuidAsync("guid_no_existente");
        });

        Assert.That(ex.Message, Is.EqualTo("Producto con guid: guid_no_existente no encontrado"));
    }
    
    [Test]
    public async Task GetByTipoAsyncProductoNotExist()
    {
        var ex = Assert.ThrowsAsync<BaseNotExistException>(async () =>
        {
            await _baseService.GetByTipoAsync("tipo_no_existente");
        });

        Assert.That(ex.Message, Is.EqualTo("Producto con tipo: tipo_no_existente no encontrado"));
    }
    
    [Test]
    public async Task CreateAsync()
    {
        var baseRequest = new BaseRequest
        {
            Nombre = "Nuevo Producto",
            TipoProducto = "Nuevo Tipo",
            Descripcion = "Descripción del producto",
            Tae = 5.0
        };

        var result = await _baseService.CreateAsync(baseRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That("Nuevo Producto", Is.EqualTo(result.Nombre));
        var productInDb = await _dbContext.ProductoBase.FirstOrDefaultAsync(p => p.Nombre == "Nuevo Producto");
        Assert.That(productInDb, Is.Not.Null);
        Assert.That("Nuevo Producto", Is.EqualTo(productInDb?.Nombre));
    }
    
    [Test]
    public async Task CreateAsyncProductTypeExists()
    {
        var baseRequest = new BaseRequest { Nombre = "Nuevo Producto", TipoProducto = "Tipo Existente" };

        var existingProduct = new BaseEntity() { Nombre = "Producto Existente", TipoProducto = "Tipo Existente" };
        _dbContext.ProductoBase.Add(existingProduct);
        await _dbContext.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<BaseExistByNameException>(() => _baseService.CreateAsync(baseRequest));
        Assert.That(ex.Message, Is.EqualTo("Ya existe un producto con el nombre: Nuevo Producto"));
    }

    [Test]
    public async Task CreateAsyncNombreExistente()
    {
        var baseRequest = new BaseRequest { Nombre = "Producto Existente", TipoProducto = "Tipo 1" };
        var existingProduct = new BaseEntity() { Nombre = "Producto Existente", TipoProducto = "Tipo 1" };
        _dbContext.ProductoBase.Add(existingProduct);
        await _dbContext.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<BaseExistByNameException>(() => _baseService.CreateAsync(baseRequest));
        Assert.That("Ya existe un producto con el nombre: Producto Existente", Is.EqualTo(ex.Message));
    }
    
    [Test]
    public async Task UpdateAsync()
    {
        var guid = Guid.NewGuid().ToString();
        var baseEntity = new BaseEntity { Guid = guid, Nombre = "Producto Original", TipoProducto = "Tipo 1" };
        _dbContext.ProductoBase.Add(baseEntity);
        await _dbContext.SaveChangesAsync();
    
        var updateRequest = new BaseUpdateDto { Nombre = "Producto Actualizado", Descripcion = "Nueva Descripción", Tae = 6.0 };

        var result = await _baseService.UpdateAsync(guid, updateRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That("Producto Actualizado", Is.EqualTo(result?.Nombre));
        Assert.That("Tipo 1", Is.EqualTo(result?.TipoProducto));
    }

    [Test]
    public async Task UpdateAsyncProductoNotExist()
    {
        var ex = Assert.ThrowsAsync<BaseNotExistException>(async () =>
        {
            await _baseService.UpdateAsync("Guid_no_existente", new BaseUpdateDto());
        });

        Assert.That(ex.Message, Is.EqualTo("Producto con guid: Guid_no_existente no encontrado"));
    }
    
    [Test]
    public async Task DeleteAsync()
    {
        var guid = Guid.NewGuid().ToString();
        var baseEntity = new BaseEntity() { Guid = guid, Nombre = "Producto a Borrar", TipoProducto = "Tipo 1", IsDeleted = false };
        _dbContext.ProductoBase.Add(baseEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _baseService.DeleteAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.IsDeleted, Is.True);
        var deletedProduct = await _dbContext.ProductoBase.FirstOrDefaultAsync(p => p.Guid == guid);
        Assert.That(deletedProduct?.IsDeleted ?? false, Is.True);
    }

    [Test]
    public async Task DeleteAsyncProductoNotExist()
    {
        var ex = Assert.ThrowsAsync<BaseNotExistException>(async () =>
        {
            await _baseService.DeleteAsync("Guid_no_existente");
        });

        Assert.That(ex.Message, Is.EqualTo("Producto con guid: Guid_no_existente no encontrado"));
    }
}

