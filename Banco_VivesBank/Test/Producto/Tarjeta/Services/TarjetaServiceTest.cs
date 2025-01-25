using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace Test.Producto.Tarjeta.Services;

[TestFixture]
public class TarjetaServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private TarjetaService _tarjetaService;
    private Mock<TarjetaGenerator> _tarjetaGeneratorMock;
    private Mock<CvvGenerator> _cvvGeneratorMock;
    private Mock<ExpDateGenerator> _expDateGeneratorMock;
    private Mock<CardLimitValidators> _cardLimitValidatorsMock;
    private Mock<IConnectionMultiplexer> _redisConnectionMock;
    private Mock<IMemoryCache> _memoryCacheMock;

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
        _tarjetaGeneratorMock = new Mock<TarjetaGenerator>();
        _cvvGeneratorMock = new Mock<CvvGenerator>();
        _expDateGeneratorMock = new Mock<ExpDateGenerator>();
        _cardLimitValidatorsMock = new Mock<CardLimitValidators>();
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _memoryCacheMock = new Mock<IMemoryCache>();

        // Crear instancia del servicio con los mocks
        _tarjetaService = new TarjetaService(
            _dbContext,
            NullLogger<TarjetaService>.Instance,
            NullLogger<CardLimitValidators>.Instance,
            _redisConnectionMock.Object, // Mock de IConnectionMultiplexer
            _memoryCacheMock.Object      // Mock de IMemoryCache
        );
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
        Assert.That(tarjeta.Guid, Is.EqualTo(guid));
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
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };


        var tarjeta = await _tarjetaService.CreateAsync(tarjetaRequest);

        Assert.That(tarjeta.Pin, Is.EqualTo("1234"));
        Assert.That(tarjeta.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async Task Update()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };

        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var tarjetaActualizada = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);

        Assert.That(tarjetaActualizada.Pin, Is.EqualTo("1234"));
        Assert.That(tarjetaActualizada.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async Task Update_NotFound()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync("non-existing-guid", tarjetaRequest);
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task UpdateInvalidLimitsDia()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task UpdateInvalidLimitsSem()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);
        Assert.That(tarjetaNoExiste, Is.Null);
    }

    [Test]
    public async Task UpdateInvalidLimitsMes()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 5000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task Delete()
    {
        var tarjeta = new TarjetaEntity
        {
            Guid = Guid.NewGuid().ToString(),
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
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        await _tarjetaService.DeleteAsync(tarjeta.Guid);

        var tarjetaBorrada = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == tarjeta.Guid);
        Assert.That(tarjetaBorrada.IsDeleted,  Is.True);
    }

    [Test]
    public async Task Delete_NotFound()
    {
        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";
        await _tarjetaService.DeleteAsync(nonExistingTarjetaGuid);

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == nonExistingTarjetaGuid);
        Assert.That(tarjetaNoExiste, Is.Null);

    }

}