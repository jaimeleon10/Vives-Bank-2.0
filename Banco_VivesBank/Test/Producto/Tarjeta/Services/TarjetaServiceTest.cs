using System.Text.Json;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Role = Banco_VivesBank.User.Models.Role;

namespace Test.Producto.Tarjeta.Services;

[TestFixture]
public class TarjetaServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private TarjetaService _tarjetaService;
    private IMemoryCache _memoryCache;
    private Mock<IConnectionMultiplexer> _redisConnectionMock;
    private Mock<IMemoryCache> _memoryCacheMock;
    private Mock<ICacheEntry> _cacheEntryMock;
    private Mock<IDatabase> _mockDatabase;

    private UserEntity user;
    private ClienteEntity cliente;
    private CuentaEntity cuenta;
    private ProductoEntity producto;
    private TarjetaEntity tarjeta;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        // Configuración del contenedor PostgreSQL
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
       
        // Crear mocks
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _mockDatabase = new Mock<IDatabase>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _redisConnectionMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_mockDatabase.Object);
        
        _tarjetaService = new TarjetaService(
            _dbContext,
            NullLogger<TarjetaService>.Instance,
            NullLogger<CardLimitValidators>.Instance,
            _redisConnectionMock.Object, // Mock de IConnectionMultiplexer
            _memoryCache
        );
    }
    
    [SetUp]
    public async Task InsertarDatos()
    {
        user = new UserEntity { Guid = "user-guid", Username = "username1", Password = "password1", Role = Role.User, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();

        cliente = new ClienteEntity
        {
            Guid = "cliente-guid", Nombre = "Juan", Apellidos = "Perez", Dni = "12345678Z",
            Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Email = "juanperez@example.com", Telefono = "600000000", IsDeleted = false, UserId = user.Id
        };
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();


        producto = new ProductoEntity { Guid = Guid.NewGuid().ToString(), Nombre = $"Producto1", TipoProducto = $"Tipo1" };
        _dbContext.ProductoBase.Add(producto);
        await _dbContext.SaveChangesAsync();
        
        tarjeta = new TarjetaEntity { Guid = "tarjeta-guid", Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23", Pin = "1234", LimiteDiario = 1000, LimiteSemanal = 2000, LimiteMensual = 3000, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();
        
        cuenta = new CuentaEntity { Guid = "cuenta-guid", Iban = "ES1234567890123456789012", Saldo = 1000, ClienteId = cliente.Id, ProductoId = producto.Id, TarjetaId = tarjeta.Id, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Cuentas.Add(cuenta);
        await _dbContext.SaveChangesAsync();
    }
    
    [TearDown]
    public async Task CleanDatabase()
    {
        // Limpia las tablas de la base de datos
        _dbContext.Cuentas.RemoveRange(await _dbContext.Cuentas.ToListAsync());
        _dbContext.Clientes.RemoveRange(await _dbContext.Clientes.ToListAsync());
        _dbContext.Usuarios.RemoveRange(await _dbContext.Usuarios.ToListAsync());
        _dbContext.ProductoBase.RemoveRange(await _dbContext.ProductoBase.ToListAsync());
        _dbContext.Tarjetas.RemoveRange(await _dbContext.Tarjetas.ToListAsync());
    
        // Guarda los cambios para aplicar la limpieza
        await _dbContext.SaveChangesAsync();
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
        if (_memoryCache != null)
        {
            _memoryCache.Dispose();
        }
    }

    [Test]
    public async Task GetByGuid()
    {
        var guid = "Guid-Prueba";
        var tarjeta1 = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta1);
        var serializedUser = JsonSerializer.Serialize(tarjeta1);
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));
        await _dbContext.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetByGuidAsync(guid);
        Assert.That(tarjeta.Guid, Is.EqualTo(guid));
    }

    [Test]
    public async Task GetByGuid_NotFound()
    {
        var guid = "Guid-Prueba";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.GetByGuidAsync("non-existing-guid");
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task GetByNumero()
    {
        var guid = "Guid-Prueba";
        var tarjeta1 = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta1);
        await _dbContext.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetByNumeroTarjetaAsync("1234567890123456");
        Assert.That(tarjeta1.Guid, Is.EqualTo(guid));
    }
    
    [Test]
    public async Task GetByNumeroNotFound()
    {
        var numero = "Guid-Prueba";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.GetByNumeroTarjetaAsync(numero);
        Assert.That(tarjetaNoExiste, Is.Null);
    }


    [Test]
    public async Task Create()
    {

        var nuevaCuenta = new CuentaEntity
        {
            Guid = "CuentaGuidTest",
            Iban = "ES7620770024003102575766", // IBAN de ejemplo
            Saldo = 0, // Saldo inicial
            TarjetaId = null, // Sin tarjeta asociada por ahora
            ClienteId = cliente.Id, // Id de un cliente existente
            ProductoId = producto.Id, // Id de un producto existente
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _dbContext.AddAsync(nuevaCuenta);
        await _dbContext.SaveChangesAsync();

        var tarjetaRequest = new TarjetaRequest
        {
            CuentaGuid = "CuentaGuidTest",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };

        var userModel = user.ToModelFromEntity();

        var tarjeta = await _tarjetaService.CreateAsync(tarjetaRequest, userModel);

        Assert.That(tarjeta.Pin, Is.EqualTo("1234"));
        Assert.That(tarjeta.LimiteDiario, Is.EqualTo(1000));
        Assert.That(tarjeta.LimiteSemanal, Is.EqualTo(3000));
        Assert.That(tarjeta.LimiteMensual, Is.EqualTo(9000));
    }

    [Test]
    public async Task Create_CuentaAEnlazarConTarjetaNoEncontrada()
    {
        var guid = "Guid-Prueba";
        var tarjetaRequest = new TarjetaRequest
        {
            CuentaGuid = guid,
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        
        var userModel = user.ToModelFromEntity();
        
        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(() => _tarjetaService.CreateAsync(tarjetaRequest, userModel));
        Assert.That(ex.Message, Is.EqualTo("Cuenta no encontrada"));
    }

    [Test]
    public async Task Create_CuentaNoPerteneceAUsuario()
    {
        var otroUsuario = new UserEntity { Guid = "user-guid2", Username = "username2", Password = "password2", Role = Role.Cliente, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Usuarios.Add(otroUsuario);
        await _dbContext.SaveChangesAsync();
        var otroCliente = new ClienteEntity { Guid = "cliente-guid2", Nombre = "Juan", Apellidos = "Perez", Dni = "12345678Z",
            Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Email = "otrousuario@example.com", UserId = otroUsuario.Id, Telefono = "600000000", IsDeleted = false };
        _dbContext.Clientes.Add(otroCliente);
        await _dbContext.SaveChangesAsync();
        
        var tarjetaRequest = new TarjetaRequest
        {
            CuentaGuid = cuenta.Guid,
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        
        var userModel = otroUsuario.ToModelFromEntity();
        
        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(() => _tarjetaService.CreateAsync(tarjetaRequest, userModel));
        
        Assert.That(ex.Message, Is.EqualTo("El cliente no tiene la cuenta"));
        
    }

    [Test]
    public async Task Create_CuentaYaTieneTarjetaAsignada()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            CuentaGuid = cuenta.Guid,
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        
        var ex = Assert.ThrowsAsync<CuentaException>(() => _tarjetaService.CreateAsync(tarjetaRequest, user.ToModelFromEntity()));
        
        Assert.That(ex.Message, Is.EqualTo("La cuenta con guid: cuenta-guid ya tiene una tarjeta asignada"));
    }

    [Test]
    public async Task Update()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        
        var serializedUser = JsonSerializer.Serialize(tarjeta);
        
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));
        var tarjetaActualizada = await _tarjetaService.UpdateAsync(tarjeta.Guid, tarjetaRequest, user.ToModelFromEntity());

        Assert.That(tarjetaActualizada.Pin, Is.EqualTo("1234"));
        Assert.That(tarjetaActualizada.LimiteDiario, Is.EqualTo(1000));
        Assert.That(tarjetaActualizada.LimiteSemanal, Is.EqualTo(3000));
        Assert.That(tarjetaActualizada.LimiteMensual, Is.EqualTo(9000));
    }

    [Test]
    public async Task Update_NotFound()
    {
        var tarjetaRequest = new TarjetaRequestUpdate()
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync("non-existing-guid", tarjetaRequest, user.ToModelFromEntity());
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task UpdateInvalidLimitsDia()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(tarjeta.Guid, tarjetaRequest, user.ToModelFromEntity()));
        
        Assert.That(ex.Message, Is.EqualTo("Error con los limites de gasto de la tarjeta, el diario debe ser superior a 0"));
    }
    
    [Test]
    public async Task UpdateInvalidLimitsSem()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(tarjeta.Guid, tarjetaRequest, user.ToModelFromEntity()));
        
        Assert.That(ex.Message, Is.EqualTo("Error con los limites de gasto de la tarjeta, el semanal debe ser superior 3 veces al diario"));
    }

    [Test]
    public async Task UpdateInvalidLimitsMes()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 5000,
            LimiteMensual = 3000
        };
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(tarjeta.Guid, tarjetaRequest, user.ToModelFromEntity()));
        
        Assert.That(ex.Message, Is.EqualTo("Error con los limites de gasto de la tarjeta, el mensual debe ser superior 3 veces al semanal"));
    }
    
    [Test]
    public async Task Delete()
    {
        var serializedUser = JsonSerializer.Serialize(tarjeta);
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));

        await _tarjetaService.DeleteAsync(tarjeta.Guid, user.ToModelFromEntity());

        var tarjetaBorrada = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == tarjeta.Guid);
        Assert.That(tarjetaBorrada.IsDeleted,  Is.True);
    }

    [Test]
    public async Task Delete_NotFound()
    {
        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";
        await _tarjetaService.DeleteAsync(nonExistingTarjetaGuid, user.ToModelFromEntity());

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == nonExistingTarjetaGuid);
        Assert.That(tarjetaNoExiste, Is.Null);
    }

}