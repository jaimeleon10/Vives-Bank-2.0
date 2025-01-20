
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Banco_VivesBank.Test.Producto.Tarjeta.Controller;

public class TarjetaControllerTest
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
    public async Task GetAllTest()
    {
        // Arrange
        var tarjetas = new List<TarjetaEntity>
        {
            new TarjetaEntity() { Id = 1, Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23" },
            new TarjetaEntity() { Id = 2, Numero = "9876543210987654", Cvv = "456", FechaVencimiento = "12/24" }
        };

        _dbContext.Tarjetas.AddRange(tarjetas);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _tarjetaService.GetAllAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdTest()
    {
        var guid = "Guid-Prueba";
        // Arrange
        var tarjeta = new TarjetaEntity
            { Id = 1, Guid = guid, Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23" };

        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _tarjetaService.GetByGuidAsync(guid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Numero, Is.EqualTo("1234567890123456"));
        Assert.That(result.Cvv, Is.EqualTo("123"));
    }

    [Test]
    public async Task GutByGuidNotFound()
    {
        var guid = "Guid-Prueba";

        // Act
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaService.GetByGuidAsync(guid));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo($"No se encontró la tarjeta con el GUID: {guid}"));
    }

    [Test]
    public async Task CreateTest()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        _tarjetaGeneratorMock.Setup(x => x.GenerarTarjeta()).Returns("generated-tarjeta-guid");
        _cvvGeneratorMock.Setup(x => x.GenerarCvv()).Returns("generated-cvv");
        _expDateGeneratorMock.Setup(x => x.GenerarExpDate()).Returns("generated-exp-date");

        // Act
        var result = await _tarjetaService.CreateAsync(tarjetaRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.Not.Null);
    }

    [Test]
    public async Task CreateWithInvalidPin()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "12345",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        _tarjetaGeneratorMock.Setup(x => x.GenerarTarjeta()).Returns("generated-tarjeta-guid");
        _cvvGeneratorMock.Setup(x => x.GenerarCvv()).Returns("generated-cvv");
        _expDateGeneratorMock.Setup(x => x.GenerarExpDate()).Returns("generated-exp-date");

        // Act
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaService.CreateAsync(tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("Pin inválido"));
    }

    [Test]
    public async Task CreateWithInvalidLimites()
    {
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000000,
            LimiteMensual = 3000
        };

        _tarjetaGeneratorMock.Setup(x => x.GenerarTarjeta()).Returns("generated-tarjeta-guid");
        _cvvGeneratorMock.Setup(x => x.GenerarCvv()).Returns("generated-cvv");
        _expDateGeneratorMock.Setup(x => x.GenerarExpDate()).Returns("generated-exp-date");

        // Act
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaService.CreateAsync(tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("Límites de tarjeta inválidos"));

    }

    [Test]
    public async Task UpdateCard()
    {

        var guid = "guid-prueba";
        
        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var tarjetaEntity = new TarjetaEntity
        {
            Id = 1,
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            LimiteDiario = 500,
            LimiteSemanal = 1000,
            LimiteMensual = 1500
        };

        _dbContext.Tarjetas.Add(tarjetaEntity);
        await _dbContext.SaveChangesAsync();
        var updatedTarjeta = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);
        
        // Assert
        Assert.That(updatedTarjeta, Is.Not.Null);
        Assert.That(updatedTarjeta.LimiteDiario, Is.EqualTo(1000));
        Assert.That(updatedTarjeta.LimiteSemanal, Is.EqualTo(2000));
        Assert.That(updatedTarjeta.LimiteMensual, Is.EqualTo(3000));
    }

    [Test]
    public async Task UpdateCardWithInvalidPin()
    {
        var guid = "guid-prueba";

        var tarjetaRequest = new TarjetaRequestDto
        {
            Pin = "12345",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var tarjetaEntity = new TarjetaEntity
        {
            Id = 1,
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            LimiteDiario = 500,
            LimiteSemanal = 1000,
            LimiteMensual = 1500
        };

        _dbContext.Tarjetas.Add(tarjetaEntity);
        await _dbContext.SaveChangesAsync();
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaService.UpdateAsync(guid, tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("No se encontró la tarjeta con el GUID: " + guid));
    }

    [Test]
    public async Task DeleteCard()
    {
        var guid = "guid-prueba";

        var tarjetaEntity = new TarjetaEntity
        {
            Id = 1,
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            LimiteDiario = 500,
            LimiteSemanal = 1000,
            LimiteMensual = 1500
        };

        _dbContext.Tarjetas.Add(tarjetaEntity);
        await _dbContext.SaveChangesAsync();
        await _tarjetaService.DeleteAsync(guid);
        var tarjeta = await _dbContext.Tarjetas.FirstOrDefaultAsync(x => x.Guid == guid);

        // Assert
        Assert.That(tarjeta, Is.Null);
    }

    [Test]
    public async Task DeleteCardNotFound()
    {
        var guid = "guid-prueba";

        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaService.DeleteAsync(guid));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("No se encontró la tarjeta con el GUID: " + guid));

    }
}