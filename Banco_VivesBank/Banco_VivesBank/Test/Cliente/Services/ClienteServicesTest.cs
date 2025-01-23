using System.Reflection;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Storage.Files.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private  IMemoryCache _memoryCache;
    private Mock<IFileStorageService> _storageService;

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
        _clienteService = new ClienteService(_dbContext, NullLogger<ClienteService>.Instance, _userServiceMock.Object, _storageService.Object, _memoryCache);
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
            UserId = 1
        };
        
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var savedCliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Guid == "cliente-guid");
        Console.WriteLine(savedCliente?.Guid);  

        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);
    
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
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);
        
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
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A",
            Email = "juanperez@example.com",
            Telefono = "600000000",
            IsDeleted = false
        };

        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByGuidAsync(clienteRequest.UserGuid)).ReturnsAsync(user);
        
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
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A",
            Email = "juanaperez@example.com",
            Telefono = "600100000",
            IsDeleted = false
        };

        _userServiceMock.Setup(x => x.GetUserModelByGuidAsync(clienteRequest.UserGuid)).ReturnsAsync((Banco_VivesBank.User.Models.User?)null);
        
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
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);

        var updateRequest = new ClienteRequestUpdate
        {
            Dni = "87654321A",
            Nombre = "Juanito",
            Apellidos = "Lopez",
            Email = "juanito@example.com",
            Telefono = "600111222",
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
    public async Task Update_ClienteNotFound()
    {
        var result = await _clienteService.UpdateAsync("non-existing-guid", new ClienteRequestUpdate());
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Update_WhenDniAlreadyExists()
    {
        var existingCliente = new ClienteEntity
        {
            Guid = "cliente-guid-1",
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
            IsDeleted = false,
            UserId = 1
        };
        _dbContext.Clientes.Add(existingCliente);
        await _dbContext.SaveChangesAsync(); 

        var clienteToUpdate = new ClienteEntity
        {
            Guid = "update-guid-1",
            Dni = "87654321X",  
            Nombre = "Carlos",
            Apellidos = "Gomez",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            Email = "carlosgomez@example.com", 
            Telefono = "600000001", 
            IsDeleted = false,
            UserId = 1
        };
        _dbContext.Clientes.Add(clienteToUpdate); 
        await _dbContext.SaveChangesAsync();

        var updateRequest = new ClienteRequestUpdate
        {
            Dni = "12345678Z", 
            Email = "newemail@example.com",
            Telefono = "600000003",
            Nombre = "Nuevo",
            Apellidos = "Nombre",
            Calle = "Calle Nueva",
            Numero = "1",
            CodigoPostal = "12345",
            Piso = "2",
            Letra = "B"
        };
        
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
                _clienteService.UpdateAsync("update-guid-1", updateRequest) 
        );

        Assert.That(ex?.Message, Is.EqualTo("Ya existe un cliente con el DNI: 12345678Z")); 
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
            UserId = 1
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);
        
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
    
  /* [Test]
    public void Validate_WhenDniAlreadyExists()
    {
        var cliente1 = new ClienteEntity
        {
            Guid = "cliente-guid-1",
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
            Email = "juanasdperez@example.com",
            Telefono = "601000000",
            IsDeleted = false,
            UserId = 1
        };
        _dbContext.Clientes.Add(cliente1);
        _dbContext.SaveChanges();
        
        var ex = Assert.Throws<TargetInvocationException>(() => 
            _clienteService.GetType()
                .GetMethod("ValidateClienteExistente", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_clienteService, new object[] { "12345678Z", "juanasdperez@example.com", "601000000" })
        );

        Assert.That(ex?.InnerException, Is.InstanceOf<ClienteExistsException>());
        Assert.That(ex?.InnerException?.Message, Is.EqualTo("Ya existe un cliente con el DNI: 12345678Z"));
    }*/

   /* [Test]
    public void Validate_WhenEmailAlreadyExists()
    {
        var existingCliente = new ClienteEntity
        {
            Guid = "emailexists",
            Dni = "98761234G",
            Nombre = "Juan",
            Apellidos = "Perez",
            Email = "juanperez@example.com",
            Telefono = "609000000",
            IsDeleted = false
        };
        _dbContext.Clientes.Add(existingCliente);
        _dbContext.SaveChanges();
        
        var ex = Assert.Throws<TargetInvocationException>(() => 
            _clienteService.GetType()
                .GetMethod("ValidateClienteExistente", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_clienteService, new object[] { "98761234G", "juanperez@example.com", "609000000" })
        );
        
        Assert.That(ex?.InnerException, Is.InstanceOf<ClienteExistsException>());
        Assert.That(ex?.InnerException?.Message, Is.EqualTo("Ya existe un cliente con el email: juanperez@example.com"));
    }

    [Test]
    public void Validate_WhenTelefonoAlreadyExists()
    {
        var existingCliente = new ClienteEntity
        {
            Guid = "telefonoExist",
            Dni = "uniqueDni",
            Nombre = "Juan",
            Apellidos = "Perez",
            Email = "anotheremail@example.com",
            Telefono = "600000000",
            IsDeleted = false
        };
        _dbContext.Clientes.Add(existingCliente);
        _dbContext.SaveChanges();
        
        var ex = Assert.Throws<TargetInvocationException>(() => 
            _clienteService.GetType()
                .GetMethod("ValidateClienteExistente", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_clienteService, new object[] { "uniqueDni", "qanotheremail@example.com", "600000000" })
        );

        Assert.That(ex?.InnerException, Is.InstanceOf<ClienteExistsException>());
        Assert.That(ex?.InnerException?.Message, Is.EqualTo("Ya existe un cliente con el teléfono: 600000000"));
    } */
}