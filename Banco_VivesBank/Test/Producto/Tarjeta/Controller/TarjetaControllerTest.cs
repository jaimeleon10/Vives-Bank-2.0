
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Controllers;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Utils.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Testcontainers.PostgreSql;

namespace Test;

public class TarjetaControllerTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private Mock<ITarjetaService> _tarjetaService;
    private TarjetaController _tarjetaController;
    private Mock<PaginationLinksUtils> _paginationLinksUtils;
    
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
        
        _tarjetaService = new Mock<ITarjetaService>();
        _paginationLinksUtils = new Mock<PaginationLinksUtils>();
        
        _tarjetaController = new TarjetaController(
            _tarjetaService.Object,
            NullLogger<CardLimitValidators>.Instance,
            _paginationLinksUtils.Object
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
        var tarjetas = new List<TarjetaResponse>
        {
            new TarjetaResponse() { Id = 1, Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23" },
            new TarjetaResponse() { Id = 2, Numero = "9876543210987654", Cvv = "456", FechaVencimiento = "12/24" }
        };

        // Act
        _tarjetaService.Setup(s => s.GetAllAsync()).ReturnsAsync(tarjetas);
        var result = await _tarjetaController.GetAllTarjetas();

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (result.Result as OkObjectResult)?.Value as IEnumerable<TarjetaResponse>;
        Assert.That(returnValue, Is.EqualTo(tarjetas));
    }

    [Test]
    public async Task GetByIdTest()
    {
        var guid = "Guid-Prueba";
        // Arrange
        var tarjeta = new TarjetaEntity
            { Id = 1, Guid = guid, Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23" };
        
        // Act
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid)).ReturnsAsync(tarjeta.ToResponseFromEntity());
        var result = await _tarjetaController.GetTarjetaById(guid);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (result.Result as OkObjectResult)?.Value as TarjetaResponse;
        Assert.That(returnValue.Id, Is.EqualTo(tarjeta.ToResponseFromEntity().Id));
    }

    [Test]
    public async Task GetByGuidNotFound()
    {
        var guid = "Guid-Prueba";
        // Act
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid)).ReturnsAsync((TarjetaResponse)null);
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaController.GetTarjetaById(guid));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo($"La tarjeta con id: {guid} no se ha encontrado"));
    }

    [Test]
    public async Task CreateTest()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        // Act
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest)).
            ReturnsAsync(tarjetaRequest.ToModelFromRequest().ToResponseFromModel);
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);
        

        // Assert
        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var returnValue = (result.Result as CreatedAtActionResult)?.Value as TarjetaResponse;
        Assert.That(returnValue, Is.Not.Null);
    }

    [Test]
    public async Task CreateWithInvalidPin()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "12345",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        // Act
        
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest)).ReturnsAsync((TarjetaResponse)null);
        
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaController.CreateTarjeta(tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("El pin tiene un formato incorrecto"));
    }

    [Test]
    public async Task CreateWithInvalidLimites()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0
        };
        // Act
        
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest)).ReturnsAsync((TarjetaResponse)null);
        
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaController.CreateTarjeta(tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo("Error con los limites de gasto de la tarjeta"));

    }

    [Test]
    public async Task UpdateCard()
    {

        var guid = "guid-prueba";
        
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid))
            .ReturnsAsync(tarjetaRequest.ToModelFromRequest().ToResponseFromModel);

        _tarjetaService.Setup(s => s.UpdateAsync(guid, tarjetaRequest))
            .ReturnsAsync(tarjetaRequest.ToModelFromRequest().ToResponseFromModel);
        
        var updatedTarjeta = await _tarjetaController.UpdateTarjeta(guid, tarjetaRequest);
        
        // Assert
        Assert.That(updatedTarjeta.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (updatedTarjeta.Result as OkObjectResult)?.Value as TarjetaResponse;
        Assert.That(returnValue, Is.Not.Null);
        Assert.That(returnValue.Pin, Is.EqualTo(tarjetaRequest.Pin));
        Assert.That(returnValue.LimiteDiario, Is.EqualTo(tarjetaRequest.LimiteDiario));
        Assert.That(returnValue.LimiteSemanal, Is.EqualTo(tarjetaRequest.LimiteSemanal));
        Assert.That(returnValue.LimiteMensual, Is.EqualTo(tarjetaRequest.LimiteMensual));

    }

    [Test]
    public async Task UpdateCardWithInvalidPin()
    {
        var guid = "guid-prueba";

        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "12345",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid))
            .ReturnsAsync((TarjetaResponse)null);

        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () =>
            await _tarjetaController.UpdateTarjeta(guid, tarjetaRequest));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo($"La tarjeta con id: {guid} no se ha encontrado"));
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
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid)).ReturnsAsync(tarjetaEntity.ToResponseFromEntity);
        _tarjetaService.Setup(s => s.DeleteAsync(guid)).ReturnsAsync(tarjetaEntity.ToResponseFromEntity);
        var tarjeta = await _tarjetaController.DeleteTarjeta(guid);

        // Assert
        Assert.That(tarjeta, Is.Not.Null);
    }

    [Test]
    public async Task DeleteCardNotFound()
    {
        var guid = "guid-prueba";

        _tarjetaService.Setup(s => s.GetByGuidAsync(guid)).ReturnsAsync((TarjetaResponse)null);
        _tarjetaService.Setup(s => s.DeleteAsync(guid)).ReturnsAsync((TarjetaResponse)null);
        

        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () => await  _tarjetaController.DeleteTarjeta(guid));

        // Assert
        Assert.That(ex?.Message, Is.EqualTo($"La tarjeta con id: {guid} no se ha encontrado"));

    }
}