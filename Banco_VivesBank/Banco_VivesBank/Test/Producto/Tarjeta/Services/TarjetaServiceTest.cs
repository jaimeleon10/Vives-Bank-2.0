using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Banco_VivesBank.Test.Producto.Tarjeta.Services;

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

        _tarjetaGeneratorMock = new Mock<TarjetaGenerator>();
        _cvvGeneratorMock = new Mock<CvvGenerator>();
        _expDateGeneratorMock = new Mock<ExpDateGenerator>();
        _cardLimitValidatorsMock = new Mock<CardLimitValidators>();
        
        _tarjetaService = new TarjetaService(_dbContext, 
            NullLogger<TarjetaService>.Instance
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
    public async Task GetAll()
    {
        var tarjeta1 = new TarjetaEntity
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

        _dbContext.Tarjetas.Add(tarjeta1);
        await _dbContext.SaveChangesAsync();

        var tarjetas = await _tarjetaService.GetAllAsync();
        Assert.That(tarjetas.Count, Is.EqualTo(1));
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
    public async void GetByGuid_NotFound()
    {
        var guid = "Guid-Prueba";
        Assert.That(async () => await _tarjetaService.GetByGuidAsync(guid), Throws.InstanceOf<TarjetaNotFoundException>());

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.GetByGuidAsync("non-existing-guid");
        Assert.That(tarjetaNoExiste, Is.Null);
    }

    [Test]
    public async Task Create()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        _tarjetaGeneratorMock.Setup(x => x.GenerarTarjeta()).Returns("tarjeta-generada");
        _cvvGeneratorMock.Setup(x => x.GenerarCvv()).Returns("cvv-generado");
        _expDateGeneratorMock.Setup(x => x.GenerarExpDate()).Returns("exp-date-generado");

        var tarjeta = await _tarjetaService.CreateAsync(tarjetaRequest);

        Assert.That(tarjeta.Numero, Is.EqualTo("tarjeta-generada"));
        Assert.That(tarjeta.Cvv, Is.EqualTo("cvv-generado"));
        Assert.That(tarjeta.FechaVencimiento, Is.EqualTo("exp-date-generado"));
        Assert.That(tarjeta.Pin, Is.EqualTo("1234"));
        Assert.That(tarjeta.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async Task Update()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

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

        var tarjetaActualizada = await _tarjetaService.UpdateAsync(tarjeta.Guid, tarjetaRequest);

        Assert.That(tarjetaActualizada.Pin, Is.EqualTo("1234"));
        Assert.That(tarjetaActualizada.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async void Update_NotFound()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";
        Assert.That(async () => await _tarjetaService.UpdateAsync(nonExistingTarjetaGuid, tarjetaRequest),
            Throws.InstanceOf<TarjetaNotFoundException>());

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync("non-existing-guid", tarjetaRequest);
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

        var tarjetaBorrada = await _dbContext.Tarjetas.FindAsync(tarjeta.Guid);
        Assert.That(tarjetaBorrada, Is.Null);
    }

    [Test]
    public async void Delete_NotFound()
    {
        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";
        await _tarjetaService.DeleteAsync(nonExistingTarjetaGuid);

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _dbContext.Tarjetas.FindAsync(nonExistingTarjetaGuid);
        Assert.That(tarjetaNoExiste, Is.Null);

    }
    
}