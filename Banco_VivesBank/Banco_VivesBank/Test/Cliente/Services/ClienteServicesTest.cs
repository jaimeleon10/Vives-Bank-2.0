using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Banco_VivesBank.Test.Cliente.Services;

[TestFixture]
public class ClienteServiceTests
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private ClienteService _clienteService;
    private Mock<IUserService> _userServiceMock;

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

        _userServiceMock = new Mock<IUserService>();
        _clienteService = new ClienteService(_dbContext, NullLogger<ClienteService>.Instance, _userServiceMock.Object);
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
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",  
            Nombre = "Juan",
            Apellidos = "Perez",
            Dni = "12345678Z",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };
        
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var savedCliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Guid == "cliente-guid");
        Console.WriteLine(savedCliente?.Guid);  

        var user = new User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelById(cliente.UserId)).ReturnsAsync(user);
    
        var result = await _clienteService.GetAllAsync();
    
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(3));
       
    }
    
    [Test]
    public async Task GetByGuid()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Nombre = "Juan",
            Apellidos = "Perez",
            Dni = "12345678Z",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelById(cliente.UserId)).ReturnsAsync(user);
        
        var result = await _clienteService.GetByGuidAsync(cliente.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cliente.Guid));
    }

    [Test]
    public async Task GetByGuid_ClienteNotExist()
    {
        var result = await _clienteService.GetByGuidAsync("non-existing-guid");
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Create()
    {
        var clienteRequest = new ClienteRequest
        {
            UserGuid = "user-guid",
            Dni = "12345678Z",
            Nombre = "Juan",
            Apellidos = "Perez",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false
        };

        var user = new User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByGuid(clienteRequest.UserGuid)).ReturnsAsync(user);
        
        var result = await _clienteService.CreateAsync(clienteRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Dni, Is.EqualTo(clienteRequest.Dni));
        Assert.That(result.Email, Is.EqualTo(clienteRequest.Email));
    }

    [Test]
    public async Task Create_UserNotExists()
    {
        var clienteRequest = new ClienteRequest
        {
            UserGuid = "non-existing-user-guid",
            Dni = "12345678A",
            Nombre = "Juana",
            Apellidos = "Perez",
            Email = "juanaperez@example.com",
            Telefono = "600100000",
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetUserModelByGuid(clienteRequest.UserGuid)).ReturnsAsync((User.Models.User?)null);
        
        var ex = Assert.ThrowsAsync<UserNotFoundException>(async () => await _clienteService.CreateAsync(clienteRequest));
        Assert.That(ex.Message, Is.EqualTo($"Usuario no encontrado con guid: {clienteRequest.UserGuid}"));
    }
    
    [Test]
    public async Task Update()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Nombre = "Juan",
            Apellidos = "Perez",
            Dni = "12345678Z",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelById(cliente.UserId)).ReturnsAsync(user);

        var updateRequest = new ClienteRequestUpdate
        {
            Dni = "87654321A",
            Nombre = "Juanito",
            Apellidos = "Lopez",
            Email = "juanito@example.com",
            Telefono = "600111222",
            IsDeleted = false,
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A"
        };
        
        var result = await _clienteService.UpdateAsync(cliente.Guid, updateRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(updateRequest.Nombre));
        Assert.That(result.Dni, Is.EqualTo(updateRequest.Dni));
    }
    [Test]
    public async Task DeleteByGuid()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Nombre = "Juan",
            Apellidos = "Perez",
            Dni = "12345678Z",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelById(cliente.UserId)).ReturnsAsync(user);
        
        var result = await _clienteService.DeleteByGuidAsync(cliente.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteByGuid_ClienteNotExist()
    {
        var result = await _clienteService.DeleteByGuidAsync("non-existing-guid");
        
        Assert.That(result, Is.Null);
    }
}