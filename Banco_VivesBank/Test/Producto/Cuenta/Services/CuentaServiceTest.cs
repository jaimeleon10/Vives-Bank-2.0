using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using BigInteger = System.Numerics.BigInteger;
using Direccion = Banco_VivesBank.Cliente.Models.Direccion;
using Role = Banco_VivesBank.User.Models.Role;

namespace Test.Producto.Cuenta.Services;

[TestFixture]
[TestOf(typeof(CuentaService))]
public class CuentaServiceTests
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private CuentaService _cuentaService;
    private Mock<IProductoService> _baseService;
    private Mock<IClienteService> _clienteService;
    private Mock<ITarjetaService> _tarjetaService;
    private Mock<IMemoryCache> _memoryCache;
    private Mock<IConnectionMultiplexer> _redis;
    private Mock<IDatabase> _database;
    private ClienteEntity _clienteEntity;
    private UserEntity user1;
    private ProductoEntity _productoEntity;
    private CuentaEntity cuenta1;
    private DbContextOptions<GeneralDbContext> _dbContextOptions;
    

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

        _dbContextOptions = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new GeneralDbContext(_dbContextOptions);
        await _dbContext.Database.EnsureCreatedAsync();

        _baseService = new Mock<IProductoService>();
        _clienteService = new Mock<IClienteService>();
        _tarjetaService = new Mock<ITarjetaService>();
        

        _memoryCache = new Mock<IMemoryCache>();

       
        _database = new Mock<IDatabase>();
        _redis = new Mock<IConnectionMultiplexer>();
        _redis.
            Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);
        
        
        _cuentaService = new CuentaService(
            _dbContext,
            NullLogger<CuentaService>.Instance,
            _baseService.Object,
            _clienteService.Object,
            _tarjetaService.Object,
            _redis.Object,
            _memoryCache.Object
        );
        
        user1 = new UserEntity
        {
            Id = 4L,
            Guid =  "user-guid",
            Username = "username1",
            Password = "password1",
            Role = Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _clienteEntity = new ClienteEntity
        {
            Id = 4L,
            Guid = "cliente-guid",
            Nombre = "Juan",
            Apellidos = "Perez",
            Dni = "12345678Z",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = user1.Id
        };
        
        _productoEntity = new ProductoEntity
        {
            Id = 4L,
            Guid = Guid.NewGuid().ToString(),
            Nombre = $"Producto1",
            TipoProducto = $"Tipo1"
        };
        
        cuenta1 = new CuentaEntity
        {
            Id = 4L,
            Guid =  "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            ClienteId = _clienteEntity.Id,
            ProductoId = _productoEntity.Id,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        InsertarDatos();
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Cuentas, ProductoBase, Clientes, Usuarios RESTART IDENTITY CASCADE");
            await _dbContext.DisposeAsync();
        }

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }
    
    private async Task InsertarDatos()
    {
        using (var scope = new GeneralDbContext(_dbContextOptions))
        {
            scope.Usuarios.Add(user1);
            await scope.SaveChangesAsync();

            scope.Clientes.Add(_clienteEntity);
            await scope.SaveChangesAsync();

            scope.ProductoBase.Add(_productoEntity);
            await scope.SaveChangesAsync();

            scope.Cuentas.Add(cuenta1);
            await scope.SaveChangesAsync();
        }
    }

    [Test]
    public async Task GetAll()
    {
        /*await InsertarDatos();

        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "Id",
            Direction = "ASC"
        };

        var result = await _cuentaService.GetAllAsync(1500, 500, "Tipo1", pageRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(BigInteger.Parse(result.Content.First().Saldo), Is.EqualTo(BigInteger.Parse("1000")));
        Assert.That(result.TotalPages, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(pageRequest.PageSize));
        Assert.That(result.PageNumber, Is.EqualTo(pageRequest.PageNumber));
        Assert.That(result.First, Is.True);
        Assert.That(result.Last, Is.True);*/
    }

    [Test]
    public async Task GetByClientGuidOk()
    {
        /*
        InsertarDatos();
        var clienteGuid = _clienteEntity.Guid;

        var result = await _cuentaService.GetByClientGuidAsync(clienteGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));

        var firstCuenta = result.First();
        Assert.That(firstCuenta.Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(firstCuenta.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(BigInteger.Parse(firstCuenta.Saldo), Is.EqualTo(1000));*/
    }

    [Test]
    public void GetByClientGuidException()
    {
        var clienteGuidInexistente = "guid-invalido";

        var exception = Assert.ThrowsAsync<ClienteNotFoundException>(async () => 
            await _cuentaService.GetByClientGuidAsync(clienteGuidInexistente)
        );

        Assert.That(exception.Message, Is.EqualTo("No se encontró el cliente con guid: guid-invalido"));
    }
/*
    [Test]
    public async Task GetByGuidMemoria()
    {
        var cuentaGuid = "cuenta-guid";
        var cuenta = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Guid = cuentaGuid,
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            Tarjeta = null,
            Cliente = new Banco_VivesBank.Cliente.Models.Cliente
            {
                Guid = "cliente-guid",
                Nombre = "Juan",
                Apellidos = "Perez",
                Dni = "12345678Z",
                Direccion = new Direccion
                {
                    Calle = "Calle Falsa",
                    Numero = "123",
                    CodigoPostal = "28000",
                    Piso = "2",
                    Letra = "A"
                },
                Email = "juanperez@example.com",
                Telefono = "600000000",
                IsDeleted = false
            },
            Producto = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
            {
                Guid = "producto-guid",
                Nombre = "Producto1",
                TipoProducto = "Tipo1"
            },
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = cuentaGuid,
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };
        
        _memoryCache
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Producto.Cuenta.Models.Cuenta>.IsAny))
            .Returns(true)
            .Callback((object key, out Banco_VivesBank.Producto.Cuenta.Models.Cuenta cachedCuenta) =>
            {
                cachedCuenta = cuenta; 
            });

        var result = await _cuentaService.GetByGuidAsync(cuentaGuid);

        Assert.That(result, Is.EqualTo(cuentaResponse));
    }

    [Test]
    public async Task GetByGuidRedis()
    {
        var cuentaGuid = "cuenta-guid";
        var cuentaResponse = new CuentaResponse
        {
            Guid = cuentaGuid,
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        var redisValue = JsonSerializer.Serialize(cuentaResponse);

        _memoryCache
            .Setup(m => m.TryGetValue(It.IsAny<object>(),
                out It.Ref<Banco_VivesBank.Producto.Cuenta.Models.Cuenta>.IsAny))
            .Returns(false);
        
        _database
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        var result = await _cuentaService.GetByGuidAsync(cuentaGuid);

        Assert.That(result, Is.EqualTo(cuentaResponse));
    }*/
    
    [Test]
    public async Task GetByGuidEnBBDD()
    {
        var guid = "cuenta-guid";
        var cuentaEntity = new CuentaEntity
        {
            Guid = guid,
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            Tarjeta = null,
            ClienteId = _clienteEntity.Id,
            ProductoId = _productoEntity.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);
        
        _database
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        await _dbContext.Cuentas.AddAsync(cuentaEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.GetByGuidAsync(guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cuentaEntity.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaEntity.Iban));
    }
    
    [Test]
    public async Task GetByGuidNoEncontrado()
    {
        var guid = "non-existent-guid";
        var cacheKey = "CachePrefix_" + guid;

        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _cuentaService.GetByGuidAsync(guid);

        Assert.That(result, Is.Null);
    }

/*
    [Test]
    public async Task GetByIbanMemoria()
    {
        var iban = "ES1234567890123456789012";
        var cuenta = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = 1000,
            Tarjeta = null,
            Cliente = new Banco_VivesBank.Cliente.Models.Cliente
            {
                Guid = "cliente-guid",
                Nombre = "Juan",
                Apellidos = "Perez",
                Dni = "12345678Z",
                Direccion = new Direccion
                {
                    Calle = "Calle Falsa",
                    Numero = "123",
                    CodigoPostal = "28000",
                    Piso = "2",
                    Letra = "A"
                },
                Email = "juanperez@example.com",
                Telefono = "600000000",
                IsDeleted = false
            },
            Producto = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
            {
                Guid = "producto-guid",
                Nombre = "Producto1",
                TipoProducto = "Tipo1"
            },
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = "1000",
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };
    
        _memoryCache
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Producto.Cuenta.Models.Cuenta>.IsAny))
            .Returns(true)
            .Callback((object key, out Banco_VivesBank.Producto.Cuenta.Models.Cuenta cachedCuenta) =>
            {
                cachedCuenta = cuenta; 
            });

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result, Is.EqualTo(cuentaResponse));
    }

    [Test]
    public async Task GetByIbanRedis()
    {
        var iban = "ES1234567890123456789012";
        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = "1000",
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        var redisValue = JsonSerializer.Serialize(cuentaResponse);

        _memoryCache
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Producto.Cuenta.Models.Cuenta>.IsAny))
            .Returns(false);
    
        _database
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result, Is.EqualTo(cuentaResponse));
    }*/

    [Test]
    public async Task GetByIbanEnBBDD()
    {
        var iban = "ES1234567890123456789012";  
        var cuentaEntity = new CuentaEntity
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = 1000,
            Tarjeta = null,
            ClienteId = 1L,
            ProductoId = 1L,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _dbContext.Cuentas.AddAsync(cuentaEntity);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cuentaEntity.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaEntity.Iban));
    }

    [Test]
    public async Task GetByIbanNoEncontrado()
    {
        var iban = "iban-no-existente";  
        var cacheKey = "CachePrefix_" + iban;

        _memoryCache
            .Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateCuentaExito()
    {
        var tipoCuenta = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Guid = "producto-guid",
            Nombre = "Producto1",
            TipoProducto = "Tipo1",
            Tae = 1.2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var baseResponse = new ProductoResponse
        {
            Guid = "producto-guid",
            Nombre = "Producto1",
            Descripcion = "Este es un producto de prueba",
            TipoProducto = "Tipo1",
            Tae = 3.5,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };
        
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Juan",
            Apellidos = "Perez",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            Email = "juanperez@example.com",
            Telefono = "600000000",
            FotoPerfil = "https://example.com/fotoPerfil.jpg",
            FotoDni = "https://example.com/fotoDni.jpg",
            User =new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "juanperez",
                Password = "securepassword123",
                Role = Role.User,  
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "Tipo1",
            ClienteGuid = _clienteEntity.Guid
        };

        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync(baseResponse);

        _baseService
            .Setup(bs => bs.GetBaseModelByGuid(tipoCuenta.Guid))
            .ReturnsAsync(tipoCuenta);

        _clienteService
            .Setup(cs => cs.GetClienteModelByGuid(cuentaRequest.ClienteGuid))
            .ReturnsAsync(cliente);

        var expectedCuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = _clienteEntity.Guid,
            ProductoGuid = _productoEntity.Guid,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };

        var result = await _cuentaService.CreateAsync(cuentaRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(expectedCuentaResponse.Guid));
        Assert.That(result.Iban, Is.EqualTo(expectedCuentaResponse.Iban));
        
    }
    
    [Test]
    public async Task CreateCuentaTipoNoExistente()
    {
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "TipoInexistente",
            ClienteGuid = "cliente-guid"
        };

        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync((ProductoResponse)null);  

        var exception = Assert.ThrowsAsync<ProductoNotExistException>(async () => await _cuentaService.CreateAsync(cuentaRequest));
        Assert.That(exception.Message, Is.EqualTo("El tipo de Cuenta TipoInexistente no existe en nuestro catalogo"));
    }

    [Test]
    public async Task CreateCuentaClienteNoExistente()
    {
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "Tipo1",
            ClienteGuid = "cliente-inexistente"
        };

        var tipoCuenta = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Guid = "producto-guid",
            Nombre = "Producto1",
            TipoProducto = "Tipo1",
            Tae = 1.2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var baseResponse = new ProductoResponse
        {
            Guid = "producto-guid",
            Nombre = "Producto1",
            Descripcion = "Este es un producto de prueba",
            TipoProducto = "Tipo1",
            Tae = 3.5,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };

        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync(baseResponse);

        _baseService
            .Setup(bs => bs.GetBaseModelByGuid(tipoCuenta.Guid))
            .ReturnsAsync(tipoCuenta);

        _clienteService
            .Setup(cs => cs.GetClienteModelByGuid(cuentaRequest.ClienteGuid))
            .ReturnsAsync((Banco_VivesBank.Cliente.Models.Cliente)null);  // Simulando que el cliente no existe

        var exception = Assert.ThrowsAsync<ClienteNotFoundException>(async () => await _cuentaService.CreateAsync(cuentaRequest));
        Assert.That(exception.Message, Is.EqualTo("El cliente cliente-inexistente no existe"));
    }


        
}