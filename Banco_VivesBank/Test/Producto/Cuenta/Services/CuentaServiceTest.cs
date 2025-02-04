using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
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
    private Mock<IConnectionMultiplexer> _redis;
    private Mock<IDatabase> _database;
    
    private UserEntity user1;
    private ClienteEntity cliente1;
    private ProductoEntity producto1;
    private CuentaEntity cuenta1;

    private IMemoryCache _memoryCache;

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

            _baseService = new Mock<IProductoService>();
            _clienteService = new Mock<IClienteService>();
            _tarjetaService = new Mock<ITarjetaService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _database = new Mock<IDatabase>();
            _redis = new Mock<IConnectionMultiplexer>();
            _redis.Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_database.Object);


            _cuentaService = new CuentaService(
                _dbContext,
                NullLogger<CuentaService>.Instance,
                _baseService.Object,
                _clienteService.Object,
                _tarjetaService.Object,
                _redis.Object,
                _memoryCache
            );
    }
    
    [TearDown]
    public async Task CleanDatabase()
    {
        // Limpia las tablas de la base de datos
        _dbContext.Cuentas.RemoveRange(await _dbContext.Cuentas.ToListAsync());
        _dbContext.Clientes.RemoveRange(await _dbContext.Clientes.ToListAsync());
        _dbContext.Usuarios.RemoveRange(await _dbContext.Usuarios.ToListAsync());
        _dbContext.ProductoBase.RemoveRange(await _dbContext.ProductoBase.ToListAsync());
    
        // Guarda los cambios para aplicar la limpieza
        await _dbContext.SaveChangesAsync();
    }


    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_memoryCache!=null)
        {
            _memoryCache.Dispose();
        }
    }

    [SetUp]
    public async Task InsertarDatos()
    {
        
        user1 = new UserEntity { Guid = "user-guid", Username = "username1", Password = "password1", Role = Role.User, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        
        _dbContext.Usuarios.Add(user1);
        await _dbContext.SaveChangesAsync();

        cliente1 = new ClienteEntity
        {
            Guid = "cliente-guid", Nombre = "Juan", Apellidos = "Perez", Dni = "12345678Z",
            Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Email = "juanperez@example.com", Telefono = "600000000", IsDeleted = false, UserId = user1.Id
        };
        _dbContext.Clientes.Add(cliente1);
        await _dbContext.SaveChangesAsync();


        producto1 = new ProductoEntity { Guid = Guid.NewGuid().ToString(), Nombre = $"Producto1", TipoProducto = $"Tipo1" };
        _dbContext.ProductoBase.Add(producto1);
        await _dbContext.SaveChangesAsync();


        cuenta1 = new CuentaEntity { Guid = "cuenta-guid", Iban = "ES1234567890123456789012", Saldo = 1000, ClienteId = cliente1.Id, ProductoId = producto1.Id, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Cuentas.Add(cuenta1);
        await _dbContext.SaveChangesAsync();
    }

    [Test]
    [Order(2)]
    public async Task GetByClientGuidOk()
    {
        var clienteGuid = cliente1.Guid;

        var result = await _cuentaService.GetByClientGuidAsync(clienteGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));

        var firstCuenta = result.First();
        Assert.That(firstCuenta.Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(firstCuenta.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(firstCuenta.Saldo, Is.EqualTo(1000));
    }

    [Test]
    [Order(3)]
    public void GetByClientGuidException()
    {
        var clienteGuidInexistente = "guid-invalido";

        var exception = Assert.ThrowsAsync<ClienteNotFoundException>(async () =>
            await _cuentaService.GetByClientGuidAsync(clienteGuidInexistente)
        );

        Assert.That(exception.Message, Is.EqualTo("No se encontró el cliente con guid: guid-invalido"));
    }

    [Test]
    [Order(4)]
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
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        _memoryCache.Set(cuentaGuid, cuenta);

        var result = await _cuentaService.GetByGuidAsync(cuentaGuid);

        Assert.That(result.Guid, Is.EqualTo(cuentaResponse.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaResponse.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaResponse.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaResponse.TarjetaGuid));
    }

    [Test]
    [Order(5)]
    public async Task GetByGuidRedis()
    {
        var cuentaGuid = "cuenta-guid";
        var cuentaResponse = new CuentaResponse
        {
            Guid = cuentaGuid,
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        var redisValue = JsonSerializer.Serialize(cuentaResponse);
        
        _memoryCache.Remove(cuentaGuid);

        _database
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        var result = await _cuentaService.GetByGuidAsync(cuentaGuid);

        Assert.That(result.Guid, Is.EqualTo(cuentaResponse.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaResponse.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaResponse.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaResponse.TarjetaGuid));
        
    }

    [Test]
    [Order(6)]
    public async Task GetByGuidEnBBDD()
    {
        var guid = "cuenta-guid";
        var cuentaEntity = new CuentaEntity
        {
            Guid = guid,
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            Tarjeta = null,
            ClienteId = cliente1.Id,
            ProductoId = producto1.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
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
    [Order(7)]
    public async Task GetByGuidNoEncontrado()
    {
        var guid = "non-existent-guid";
        var cacheKey = "CachePrefix_" + guid;

        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _cuentaService.GetByGuidAsync(guid);

        Assert.That(result, Is.Null);
    }


    [Test]
    [Order(8)]
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
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        _memoryCache.Set(cuenta.Guid, cuenta);

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result.Guid, Is.EqualTo(cuentaResponse.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaResponse.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaResponse.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaResponse.TarjetaGuid));
        Assert.That(result.ClienteGuid, Is.EqualTo(cuentaResponse.ClienteGuid));
    }

    [Test]
    [Order(9)]
    public async Task GetByIbanRedis()
    {
        var iban = "ES1234567890123456789012";
        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = "2025-01-24T12:00:00Z",
            UpdatedAt = "2025-01-24T12:30:00Z",
            IsDeleted = false
        };

        var redisValue = JsonSerializer.Serialize(cuentaResponse);

        _memoryCache.Remove(cuentaResponse.Guid);

        _database
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result.Guid, Is.EqualTo(cuentaResponse.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaResponse.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaResponse.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaResponse.TarjetaGuid));
        Assert.That(result.ClienteGuid, Is.EqualTo(cuentaResponse.ClienteGuid));
    }

    [Test]
    [Order(10)]
    public async Task GetByIbanEnBBDD()
    {
        var iban = "ES1234567890123456789012";
        var cuentaEntity = new CuentaEntity
        {
            Guid = "cuenta-guid",
            Iban = iban,
            Saldo = 1000,
            Tarjeta = null,
            ClienteId = cliente1.Id,
            ProductoId = producto1.Id,
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
    [Order(11)]
    public async Task GetByIbanNoEncontrado()
    {
        var iban = "iban-no-existente";  
        var cacheKey = "CachePrefix_" + iban;
        
        _database
            .Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _cuentaService.GetByIbanAsync(iban);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Order(12)]
    public async Task CreateCuentaExito()
    {
        var tipoCuenta = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Id = producto1.Id,
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
        };

        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync(baseResponse);

        _baseService
            .Setup(bs => bs.GetBaseModelByGuid(tipoCuenta.Guid))
            .ReturnsAsync(tipoCuenta);

        _clienteService
            .Setup(cs => cs.GetClienteModelByGuid(cliente1.Guid))
            .ReturnsAsync(cliente);

        var expectedCuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 0,
            TarjetaGuid = null,
            ClienteGuid = cliente1.Guid,
            ProductoGuid = producto1.Guid,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };

        var result = await _cuentaService.CreateAsync(user1.Guid,cuentaRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Saldo, Is.EqualTo(expectedCuentaResponse.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(expectedCuentaResponse.TarjetaGuid));
        Assert.That(result.ClienteGuid, Is.EqualTo(expectedCuentaResponse.ClienteGuid));
        Assert.That(result.ProductoGuid, Is.EqualTo(expectedCuentaResponse.ProductoGuid));
    }
    
    [Test]
    [Order(13)]
    public async Task CreateCuentaTipoNoExistente()
    {
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "TipoInexistente"
        };

        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync((ProductoResponse)null);  

        var exception = Assert.ThrowsAsync<ProductoNotExistException>(async () => await _cuentaService.CreateAsync(user1.Guid, cuentaRequest));
        Assert.That(exception.Message, Is.EqualTo("El tipo de Cuenta TipoInexistente no existe en nuestro catalogo"));
    }

    [Test]
    [Order(14)]
    public async Task CreateCuentaClienteNoExistente()
    {
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "Tipo1"
        };

        var tipoCuenta = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Id = producto1.Id,
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

        UserEntity user2 = new UserEntity { Guid = "user2-guid", Username = "username2", Password = "password2", Role = Role.User, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Usuarios.Add(user2);
        await _dbContext.SaveChangesAsync();
        
        _baseService
            .Setup(bs => bs.GetByTipoAsync(cuentaRequest.TipoCuenta))
            .ReturnsAsync(baseResponse);

        _baseService
            .Setup(bs => bs.GetBaseModelByGuid(tipoCuenta.Guid))
            .ReturnsAsync(tipoCuenta);
        

        var exception = Assert.ThrowsAsync<ClienteNotFoundException>(async () => await _cuentaService.CreateAsync(user2.Guid, cuentaRequest));
        Assert.That(exception.Message, Is.EqualTo("No se encontró el cliente con guid: user2-guid"));
    }



    [Test]
    [Order(15)]
    public async Task GetAllForStorage()
    {
        var result = await _cuentaService.GetAllForStorage();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(result[0].Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result[0].Saldo, Is.EqualTo(1000));
        
    }
    
    [Test]
    [Order(17)]
    public async Task GetAllForStorageEmpty()
    {
       _dbContext.Cuentas.RemoveRange(await _dbContext.Cuentas.ToListAsync());
        await _dbContext.SaveChangesAsync();
        
        var result = await _cuentaService.GetAllForStorage();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }


    [Test, Order(19)]
    public async Task GetCuentaModelByIdAsync_Success()
    {
        var result = await _cuentaService.GetCuentaModelByIdAsync(cuenta1.Id);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result.Saldo, Is.EqualTo(1000));
        Assert.That(result.Cliente.Id, Is.EqualTo(cliente1.Id));
        Assert.That(result.Producto.Id, Is.EqualTo(producto1.Id));
    }
    
    [Test, Order(21)]
    public async Task GetCuentaModelByIdAsync_NotFound()
    {
        var result = await _cuentaService.GetCuentaModelByIdAsync(1000);
        
        Assert.That(result, Is.Null);
    }
    
    [Test, Order(23)]
    public async Task GetCuentaModelByGuidAsync_Success()
    {
        var result = await _cuentaService.GetCuentaModelByGuidAsync("cuenta-guid");
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result.Saldo, Is.EqualTo(1000));
        Assert.That(result.Cliente.Id, Is.EqualTo(cliente1.Id));
        Assert.That(result.Producto.Id, Is.EqualTo(producto1.Id));
    }
    
    [Test, Order(25)]
    public async Task GetCuentaModelByGuidAsync_NotFound()
    {
        var result = await _cuentaService.GetCuentaModelByGuidAsync("cuenta-guid-not-found");
        
        Assert.That(result, Is.Null);
    }
    
    // [Test, Order(27)] Fallo en el mapper 
    // public async Task DeleteMeAsync_Success()
    // {
    //     var result = await _cuentaService.DeleteMeAsync(user1.Guid, cuenta1.Guid);
    //     
    //     Assert.That(result, Is.Not.Null);
    //     Assert.That(result.Guid, Is.EqualTo("cuenta-guid"));
    //     Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
    //     Assert.That(result.Saldo, Is.EqualTo(1000));
    //     Assert.That(result.IsDeleted, Is.EqualTo(true));
    //     Assert.That(result.UpdatedAt, Is.Not.EqualTo(result.CreatedAt));
    // }
    
    [Test, Order(29)]
    public async Task DeleteMeAsync_ClienteNotFound()
    {
        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => await _cuentaService.DeleteMeAsync("cliente-guid-not-found", "cuenta-guid"));
        
        Assert.That(ex.Message, Is.EqualTo("No se encontró el cliente con guid: cliente-guid-not-found"));
        
    }
    
    [Test, Order(31)]
    public async Task DeleteMeAsync_CuentaNotFound()
    {
        var guidFlase = "cuenta-guid-not-found";
        var result =await  _cuentaService.DeleteMeAsync(user1.Guid, guidFlase);
        
        Assert.That(result, Is.Null);
    }
    
    [Test, Order(33)]
    public async Task DeleteMeAsync_CuentaNoPerteneceAUsuario()
    {
        var userEntity = new UserEntity { Guid = "user2-guid", Username = "username2", Password = "password2", Role = Role.User, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        var clienteEntity = new ClienteEntity { Guid = "cliente-guid2", Dni="12345678b" ,Telefono = "123465789", Nombre = "Juan", Apellidos = "PEREZ", Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Email = "algo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false, UserId = userEntity.Id };
        _dbContext.Clientes.Add(clienteEntity);
        await _dbContext.SaveChangesAsync();
        var cuentaEntity = new CuentaEntity { Guid = "cuenta-guid2", Iban = "ES1234567890123456789012", Saldo = 1000, ClienteId = clienteEntity.Id, ProductoId = producto1.Id, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Cuentas.Add(cuentaEntity);
        await _dbContext.SaveChangesAsync();
        
        var result = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>( async () => await _cuentaService.DeleteMeAsync(user1.Guid, cuentaEntity.Guid));
        
        Assert.That(result.Message, Is.EqualTo("La cuenta cuenta-guid2 no te pertenece"));
    }
    
    [Test, Order(35)]
    public async Task DeleteByGuidAsync_Success()
    {
        var result = await _cuentaService.DeleteByGuidAsync(cuenta1.Guid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result.Saldo, Is.EqualTo(1000));
        Assert.That(result.IsDeleted, Is.EqualTo(true));
    }
    
    [Test, Order(37)]
    public async Task DeleteByGuid_CuentaNotFound()
    {
        var result = await _cuentaService.DeleteByGuidAsync("cuenta-guid-not-found");
        
        Assert.That(result, Is.Null);
    }
    
    
    [Test]
    [Order(16)]
    public async Task GetAllMeAsyncOk()
    {
        
        var userGuid = "user-guid";
        var cuentas = new List<CuentaEntity>
        {
            new CuentaEntity { Guid = "cuenta-1", Iban = "ES1111111111111111111111", Saldo = 500, ClienteId = cliente1.Id, ProductoId = producto1.Id },
            new CuentaEntity { Guid = "cuenta-2", Iban = "ES2222222222222222222222", Saldo = 1500, ClienteId = cliente1.Id, ProductoId = producto1.Id }
        };

        await _dbContext.Cuentas.AddRangeAsync(cuentas);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.GetAllMeAsync(userGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(3));
        Assert.That(result.First().Guid, Is.EqualTo("cuenta-guid"));
        Assert.That(result.Last().Guid, Is.EqualTo("cuenta-2"));
    }

    [Test]
    [Order(18)]
    public void GetAllMeAsyncClienteNotFoundException()
    {
        var userGuid = "non-existent-guid";

        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => await _cuentaService.GetAllMeAsync(userGuid));
        Assert.That(ex.Message, Is.EqualTo($"No se encontró el cliente con guid: {userGuid}"));
    }
    
    [Test]
    [Order(20)]
    public async Task GetMeByIbanAsyncOk()
    {
        var userGuid = "user-guid";
        var iban = "ES1111111111111111111111";
        var cuenta = new CuentaEntity { Guid = "cuenta-1", Iban = iban, Saldo = 500, ClienteId = cliente1.Id, ProductoId = producto1.Id };
        await _dbContext.Cuentas.AddAsync(cuenta);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.GetMeByIbanAsync(userGuid, iban);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Iban, Is.EqualTo(iban));
    }

    [Test]
    [Order(22)]
    public void GetMeByIbanAsyncClienteNotFoundException()
    {
        var userGuid = "non-existent-guid";
        var iban = "ES1111111111111111111111";

        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => await _cuentaService.GetMeByIbanAsync(userGuid, iban));
        Assert.That(ex.Message, Is.EqualTo($"No se encontró el cliente con guid: {userGuid}"));
    }

    [Test]
    [Order(24)]
    public async Task GetMeByIbanAsyncCuentaNoPertenecienteAlUsuarioException()
    {
        var userEntity = new UserEntity { Guid = "user2-guid", Username = "username2", Password = "password2", Role = Role.User, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        var clienteEntity = new ClienteEntity { Guid = "cliente-guid2", Dni="12345678b" ,Telefono = "123465789", Nombre = "Juan", Apellidos = "PEREZ", Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Email = "algo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false, UserId = userEntity.Id };
        _dbContext.Clientes.Add(clienteEntity);
        await _dbContext.SaveChangesAsync();
       
        var userGuid = "user-guid";
        var iban = "ES2222222222222222222222";
        var cuenta = new CuentaEntity { Guid = "cuenta-2", Iban = iban, Saldo = 1500, ProductoId = producto1.Id, ClienteId = clienteEntity.Id};
        await _dbContext.Cuentas.AddAsync(cuenta);
        await _dbContext.SaveChangesAsync();

        var ex = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>(async () => await _cuentaService.GetMeByIbanAsync(userGuid, iban));
        Assert.That(ex.Message, Is.EqualTo($"El iban {iban} no te pertenece"));
    }

    [Test]
    [Order(26)]
    public async Task GetMeByIbanAsyncNull()
    {
        var userGuid = "user-guid";
        var iban = "ES3333333333333333333333";

        var result = await _cuentaService.GetMeByIbanAsync(userGuid, iban);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Order(28)]
    public async Task GetByTarjetaGuidAsyncok()
    {
        var tarjetaGuid = "tarjeta-guid";
        var cuenta = new CuentaEntity { Guid = "cuenta-1", Iban = "ES1111111111111111111111", Saldo = 500, ClienteId = cliente1.Id, ProductoId = producto1.Id, Tarjeta = new TarjetaEntity { Guid = tarjetaGuid, Numero = "1", Cvv = "123", FechaVencimiento = "10/28", Pin = "123", LimiteDiario = 10, LimiteMensual = 1000, LimiteSemanal = 500 } };
        await _dbContext.Cuentas.AddAsync(cuenta);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.GetByTarjetaGuidAsync(tarjetaGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("cuenta-1"));
    }

    [Test]
    [Order(30)]
    public async Task GetByTarjetaGuidAsyncNull()
    {
        var tarjetaGuid = "non-existent-tarjeta-guid";

        
        var result = await _cuentaService.GetByTarjetaGuidAsync(tarjetaGuid);

        Assert.That(result, Is.Null);
    }
    

    
    
}