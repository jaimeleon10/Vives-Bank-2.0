using System.Linq.Expressions;
using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Mappers;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace Test.Producto.Base.Service;

[TestFixture]
[TestOf(typeof(ProductoService))]
public class ProductoServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private ProductoService _productoService;
    private Mock<IMemoryCache> _memoryCache;
    private Mock<IConnectionMultiplexer> _redis;
    private Mock<IDatabase> _database;

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
        
        _memoryCache = new Mock<IMemoryCache>();
        _database = new Mock<IDatabase>();
        _redis = new Mock<IConnectionMultiplexer>();
        _redis.
            Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);

        _productoService = new ProductoService(_dbContext, NullLogger<ProductoService>.Instance, _redis.Object, _memoryCache.Object);
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
    public async Task GetAllPagedAsync_ShouldReturnPaginatedResults_WhenValidRequest()
    {
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 2,
            SortBy = "id",
            Direction = "ASC"
        };

        _dbContext.ProductoBase.AddRange(new ProductoEntity { Nombre = "1" }, new ProductoEntity { Nombre = "2" }, new ProductoEntity { Nombre = "3" });
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetAllPagedAsync(pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(2));
        Assert.That(result.PageNumber, Is.EqualTo(0));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalElements, Is.EqualTo(10));
        Assert.That(result.TotalPages, Is.EqualTo(5));
        Assert.That(result.Empty, Is.False);
        Assert.That(result.First, Is.True);
        Assert.That(result.Last, Is.False);
    }
    
    [Test]
    public async Task GetAllAsync()
    {
        await _dbContext.ProductoBase.ExecuteDeleteAsync();

        for (int i = 0; i < 5; i++)
        {
            var baseEntity = new ProductoEntity()
            {
                Guid = Guid.NewGuid().ToString(),
                Nombre = $"Producto {i + 1}",
                TipoProducto = $"Tipo {i + 1}"
            };
            _dbContext.ProductoBase.Add(baseEntity);
        }
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(5));
    }

    [Test]
    public async Task GetByGuidAsyncReturnRedis()
    {
        var guid = "some-guid";
        var cacheKey = "Producto:" + guid;
        var redisValue = JsonSerializer.Serialize(new ProductoResponse { Nombre = "Producto desde Redis" });

        _database.Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        object cachedProduct = null;

        var result = await _productoService.GetByGuidAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo("Producto desde Redis"));

        _database.Verify(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()), Times.Exactly(2)); 
    }

    [Test]
    public async Task GetByGuidAsyncReturnDatabase()
    {
        var guid = "some-guid";
        var productInDb = new Banco_VivesBank.Producto.ProductoBase.Models.Producto { Guid = guid, Nombre = "Producto desde BD", TipoProducto = "Tipo1" };
        
        await _dbContext.ProductoBase.AddAsync(productInDb.ToEntityFromModel());
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetByGuidAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo("Producto desde BD"));
    }

    [Test]
    public async Task GetByGuidAsyncProductNotExist()
    {
        var guid = "non-existent-guid";
        var cacheKey = "CachePrefix_" + guid;

        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _productoService.GetByGuidAsync(guid);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByTipoAsyncReturnRedis()
    {
        var tipo = "some-tipo";
        var cacheKey = "Producto:" + tipo;
        var redisValue = JsonSerializer.Serialize(new ProductoResponse { Nombre = "Producto desde Redis", TipoProducto = tipo});

        _database.Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        object cachedProduct = null;

        var result = await _productoService.GetByTipoAsync(tipo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo("Producto desde Redis"));

        _database.Verify(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()), Times.Once); 
    }

    [Test]
    public async Task GetByTipoAsyncReturnDatabase()
    {
        var tipo = "Tipo3";
        var productFromDb = new Banco_VivesBank.Producto.ProductoBase.Models.Producto { TipoProducto = tipo, Nombre = "Producto desde BD" };
        await _dbContext.ProductoBase.AddAsync(productFromDb.ToEntityFromModel());
        await _dbContext.SaveChangesAsync();
        
        var result = await _productoService.GetByTipoAsync(tipo);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(productFromDb.Nombre));
        Assert.That(result.TipoProducto, Is.EqualTo(productFromDb.TipoProducto));
    }
    
    [Test]
    public async Task GetByTipoAsyncProductoNotExist()
    {
        var tipo = "non-existent-tipo";
        var cacheKey = "CachePrefix_" + tipo;

        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _productoService.GetByTipoAsync(tipo);

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task CreateAsync()
    {
        var request = new ProductoRequest { Nombre = "ProductoNuevo", TipoProducto = "TipoNuevo", Descripcion = "descripcion"};

        object cacheValue = null;
        _memoryCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Callback<object>(key => cacheValue = key)
            .Returns(Mock.Of<ICacheEntry>());

        var response = await _productoService.CreateAsync(request);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Nombre, Is.EqualTo(request.Nombre));
        Assert.That(response.TipoProducto, Is.EqualTo(request.TipoProducto));

        var productInDb = await _dbContext.ProductoBase.FirstOrDefaultAsync(p => p.Nombre == request.Nombre);
        Assert.That(productInDb, Is.Not.Null);

        _database.Verify(db => db.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            When.Always,
            It.IsAny<CommandFlags>()),
            Times.Once);

        _memoryCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
    }
    
    [Test]
    public async Task CreateAsyncProductTypeExists()
    {
        var baseRequest = new ProductoRequest { Nombre = "Nuevo Producto", TipoProducto = "Tipo Existente" };

        var existingProduct = new ProductoEntity() { Nombre = "Producto Existente", TipoProducto = "Tipo Existente" };
        _dbContext.ProductoBase.Add(existingProduct);
        await _dbContext.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<ProductoDuplicatedException>(() => _productoService.CreateAsync(baseRequest));
        Assert.That(ex.Message, Is.EqualTo("Ya existe un producto con el tipo: Tipo Existente"));
    }

    [Test]
    public async Task CreateAsyncNombreExistente()
    {
        var baseRequest = new ProductoRequest { Nombre = "Producto Existente", TipoProducto = "Tipo 1" };
        var existingProduct = new ProductoEntity() { Nombre = "Producto Existente", TipoProducto = "Tipo 1" };
        _dbContext.ProductoBase.Add(existingProduct);
        await _dbContext.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<ProductoExistByNameException>(() => _productoService.CreateAsync(baseRequest));
        Assert.That("Ya existe un producto con el nombre: Producto Existente", Is.EqualTo(ex.Message));
    }
    
    [Test]
    public async Task UpdateAsync()
    {
        var guid = Guid.NewGuid().ToString();
        var baseEntity = new ProductoEntity { Guid = guid, Nombre = "Producto Original", TipoProducto = "Tipo 1" };
        _dbContext.ProductoBase.Add(baseEntity);
        await _dbContext.SaveChangesAsync();
    
        var updateRequest = new ProductoRequestUpdate { Nombre = "Producto Actualizado", Descripcion = "Nueva Descripción", Tae = 6.0 };

        var result = await _productoService.UpdateAsync(guid, updateRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That("Producto Actualizado", Is.EqualTo(result?.Nombre));
        Assert.That("Tipo 1", Is.EqualTo(result?.TipoProducto));
    }

    [Test]
    public async Task UpdateAsyncProductoNotExist()
    {
        var ex = Assert.ThrowsAsync<ProductoNotExistException>(async () =>
        {
            await _productoService.UpdateAsync("Guid_no_existente", new ProductoRequestUpdate());
        });

        Assert.That(ex.Message, Is.EqualTo("Producto con guid: Guid_no_existente no encontrado"));
    }
    
    [Test]
    public async Task UpdateAsyncBaseExistByName()
    {
        var guid = Guid.NewGuid().ToString();
    
        var existingProduct = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Guid = guid,
            Nombre = "Producto Existente",
            Descripcion = "Descripción del producto",
            TipoProducto = "NuevoTipoProducto",
            Tae = 10.0
        };
        await _dbContext.ProductoBase.AddAsync(existingProduct.ToEntityFromModel());

        var conflictingProduct = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Guid = Guid.NewGuid().ToString(),
            Nombre = "Nombre Conflictivo",
            Descripcion = "Otro producto",
            TipoProducto = "OtroTipoProducto",
            Tae = 8.5
        };
        await _dbContext.ProductoBase.AddAsync(conflictingProduct.ToEntityFromModel());
        await _dbContext.SaveChangesAsync();

        var baseUpdateDto = new ProductoRequestUpdate
        {
            Nombre = "Nombre Conflictivo",
            Descripcion = "Descripción actualizada",
            Tae = 12.0
        };

        var ex = Assert.ThrowsAsync<ProductoExistByNameException>(async () =>
            await _productoService.UpdateAsync(guid, baseUpdateDto)
        );

        Assert.That(ex.Message, Is.EqualTo($"Ya existe un producto con el nombre: {baseUpdateDto.Nombre}"));
    }
    
    [Test]
    public async Task DeleteAsync()
    {
        var guid = Guid.NewGuid().ToString();
        var baseEntity = new ProductoEntity() { Guid = guid, Nombre = "Producto a Borrar", TipoProducto = "Tipo 1", IsDeleted = false };
        _dbContext.ProductoBase.Add(baseEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.DeleteByGuidAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.IsDeleted, Is.True);
        var deletedProduct = await _dbContext.ProductoBase.FirstOrDefaultAsync(p => p.Guid == guid);
        Assert.That(deletedProduct?.IsDeleted ?? false, Is.True);
    }

    [Test]
    public async Task DeleteAsyncProductoNotExist()
    {
        var guid = "non-existent-guid";
        var cacheKey = "CachePrefix_" + guid;

        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _productoService.DeleteByGuidAsync(guid);

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetAllForStorage()
    {
        var productoEntity1 = new ProductoEntity { Nombre = "Producto1" };
        var productoEntity2 = new ProductoEntity { Nombre = "Producto2" };
        _dbContext.ProductoBase.AddRange(productoEntity1, productoEntity2);
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetAllForStorage();

        Assert.That(result.Count(), Is.EqualTo(7));
        Assert.That(result.All(item => item is Banco_VivesBank.Producto.ProductoBase.Models.Producto), Is.True);
    }
    
    [Test]
    public async Task GetBaseModelByGuid()
    {
        var guid = "guidguid";
        var productoEntity = new ProductoEntity { Guid = guid, Nombre = "Producto1" };
        _dbContext.ProductoBase.Add(productoEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetBaseModelByGuid(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Guid, Is.EqualTo(productoEntity.Guid));
        Assert.That(result?.Nombre, Is.EqualTo(productoEntity.Nombre));
    }
    
    [Test]
    public async Task GetBaseModelByGuidProductNotExist()
    {
        var guid = Guid.NewGuid().ToString();

        var result = await _productoService.GetBaseModelByGuid(guid);

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetBaseModelById()
    {
        var productoEntity = new ProductoEntity { Nombre = "Producto1" };
        _dbContext.ProductoBase.Add(productoEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _productoService.GetBaseModelById(productoEntity.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Id, Is.EqualTo(productoEntity.Id));
        Assert.That(result?.Nombre, Is.EqualTo(productoEntity.Nombre));
    }
    
    [Test]
    public async Task GetBaseModelByIdProductNotExist()
    {
        var id = 900L;

        var result = await _productoService.GetBaseModelById(id);

        Assert.That(result, Is.Null);
    }
}