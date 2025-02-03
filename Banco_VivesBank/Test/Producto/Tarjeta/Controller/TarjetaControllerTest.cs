using System.Security.Claims;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Controllers;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Utils.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Testcontainers.PostgreSql;

namespace Test.Producto.Tarjeta.Controller;

public class TarjetaControllerTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private Mock<ITarjetaService> _tarjetaService;
    private TarjetaController _tarjetaController;
    private Mock<PaginationLinksUtils> _paginationLinksUtils;
    private Mock<IUserService> _userService;
    
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
        _userService = new Mock<IUserService>();
        
        _tarjetaController = new TarjetaController(
            _tarjetaService.Object,
            NullLogger<CardLimitValidators>.Instance,
            _paginationLinksUtils.Object,
            _userService.Object
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
            new TarjetaResponse() { Numero = "1234567890123456", Cvv = "123", FechaVencimiento = "01/23" },
            new TarjetaResponse() { Numero = "9876543210987654", Cvv = "456", FechaVencimiento = "12/24" }
        };

        
        var page = 0;
        var size = 10;
        var sortBy = "id";
        var direction = "desc";
            
        var pageRequest = new PageRequest
        {
            PageNumber = page,
            PageSize = size,
            SortBy = sortBy,
            Direction = direction
        };

        var pageResponse = new PageResponse<TarjetaResponse>
        {
            Content = tarjetas,
            TotalElements = tarjetas.Count,
            PageNumber = pageRequest.PageNumber,
            PageSize = pageRequest.PageSize,
            TotalPages = 1
        };

        _tarjetaService.Setup(s => s.GetAllPagedAsync(pageRequest))
            .ReturnsAsync(pageResponse);

        var baseUri = new Uri("http://localhost/api/productosBase");
        _paginationLinksUtils.Setup(utils => utils.CreateLinkHeader(pageResponse, baseUri))
            .Returns("<http://localhost/api/productosBase?page=0&size=5>; rel=\"prev\",<http://localhost/api/productosBase?page=2&size=5>; rel=\"next\"");

        // Configurar el contexto HTTP para la prueba
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/productosBase")
            }
        };

        _tarjetaController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        // Act
        _tarjetaService.Setup(s => s.GetAllPagedAsync(pageRequest)).ReturnsAsync(pageResponse);
        var result = await _tarjetaController.GetAllTarjetas(page, size, sortBy, direction);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
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
        var result = await _tarjetaController.GetTarjetaByGuid(guid);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetByGuidNotFound()
    {
        var guid = "Guid-Prueba";
        // Act
        _tarjetaService.Setup(s => s.GetByGuidAsync(guid)).ReturnsAsync((TarjetaResponse)null);
        
        var result =  await  _tarjetaController.GetTarjetaByGuid(guid);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
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
        
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba" , Role = Role.Cliente };
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(user);
        // Act
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest,user )).
            ReturnsAsync(tarjetaRequest.ToModelFromRequest().ToResponseFromModel);
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);
        
        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (result.Result as OkObjectResult)?.Value as TarjetaResponse;
        Assert.That(returnValue, Is.Not.Null);
    }
/*
    [Test]
    public async Task CreateTarjeta_InvalidUserAdmin()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba", Role = Role.Admin, Username = "admin" };
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(user);
        
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);
        
        Assert.That(result.Result , Is.TypeOf<BadRequestObjectResult>());
        var badRequestresut = result.Result as BadRequestObjectResult;
        Assert.That(result.Value, Is.EqualTo("El usuario es administrador. Un administrador no puede crear una tarjeta"));
    }

    [Test]
    public async Task CreateTarjeta_NotFoundUser()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User?)null);
        
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);
        
        Assert.That(result.Result , Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(result.Value, Is.EqualTo("No se ha podido identificar al usuario logeado"));
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
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba" , Role = Role.Cliente };
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(user);
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest, user))!.ReturnsAsync((TarjetaResponse)null);
        
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());    
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
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba" , Role = Role.Cliente };
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(user);
        _tarjetaService.Setup(s => s.CreateAsync(tarjetaRequest, user)).ReturnsAsync((TarjetaResponse)null);
        
        var result = await _tarjetaController.CreateTarjeta(tarjetaRequest);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());    
    }
/*
    [Test]
    public async Task UpdateCard()
    {

        var guid = "guid-prueba";
        
        var tarjetaRequest = new TarjetaRequestUpdate()
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

        var ex = await _tarjetaController.UpdateTarjeta(guid, tarjetaRequest);

        // Assert
        Assert.That(ex.Result, Is.TypeOf<BadRequestObjectResult>());    
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
        var userAdmin = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba", Role = Role.Admin, Username = "admin" };
        
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(userAdmin);

        _tarjetaService.Setup(s => s.DeleteAsync(guid, userAdmin)).ReturnsAsync((TarjetaResponse)null);
        

        var result =  await  _tarjetaController.DeleteTarjeta(guid);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo($"La tarjeta con guid: {guid} no se ha encontrado"));
    }

    [Test]
    public async Task DeleteCardWithInvalidUser()
    {
        var guid = "guid-prueba";
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "guid-prueba", Role = Role.User, Username = "usuario" };
        
        _userService.Setup(s => s.GetAuthenticatedUser()).Returns(user);

        var result = await _tarjetaController.DeleteTarjeta(guid);
        
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestresut = result.Result as BadRequestObjectResult;
        Assert.That(result.Value, Is.EqualTo("Debes ser cliente o admin para borrar una tarjeta."));
    }*/
    
}