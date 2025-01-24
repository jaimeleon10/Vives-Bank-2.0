using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Storage.Files.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace Test.Cliente.Services;

[TestFixture]
public class ClienteServiceTests
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private ClienteService _clienteService;
    private Mock<IUserService> _userServiceMock;
    private Mock<IFileStorageService> _storageService;
    private Mock<IConnectionMultiplexer> _redisConnectionMock;
    private Mock<IDatabase> _redisDatabaseMock;

    private Mock<IMemoryCache> _memoryCacheMock;


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
        _storageService = new Mock<IFileStorageService>();

        _memoryCacheMock = new Mock<IMemoryCache>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _redisConnectionMock
            .Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_redisDatabaseMock.Object);
        
       
        
        _clienteService = new ClienteService(_dbContext, NullLogger<ClienteService>.Instance, _userServiceMock.Object, _storageService.Object, _memoryCacheMock.Object, _redisConnectionMock.Object);
    }
    
    [SetUp]
    public async Task CleanDatabase()
    {
        // Limpia las tablas de la base de datos
        _dbContext.Clientes.RemoveRange(_dbContext.Clientes);
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);

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
    }  
    
    [Test]
    public async Task GetAll()
    {
        var user = new UserEntity
        {
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();
        
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

    
        var result = await _clienteService.GetAllAsync();
    
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
       
    }
    
    [Test]
    public async Task GetByGuid_EnBBDD()
    {
        
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        
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
            UserId = userEntity.Id,
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);

        _redisDatabaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Cliente.Models.Cliente>.IsAny))
            .Returns(false);
        var result = await _clienteService.GetByGuidAsync(cliente.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cliente.Guid));
    }
    
    [Test]
    public async Task GetByGuid_ClienteEnMemoria()
    {
        var clienteGuid = "existing-guid";
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente"
        };
        var clienteResponse = new ClienteResponse
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente"
        };
    
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Cliente.Models.Cliente>.IsAny))
            .Returns(true)
            .Callback((object key, out Banco_VivesBank.Cliente.Models.Cliente clienteCache) =>
            {
                clienteCache = cliente;
            });
    
        var result = await _clienteService.GetByGuidAsync(clienteGuid);
    
        Assert.That(result, Is.EqualTo(clienteResponse));
    }

    [Test]
    public async Task GetByGuid_ClienteEnRedis()
    {
        // Arrange
        var clienteGuid = "existing-guid";
        var clienteResponse = new ClienteResponse
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente"
        };

        var redisValue = JsonSerializer.Serialize(clienteResponse);

        // Mock de Redis
        _redisDatabaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        // Act
        var result = await _clienteService.GetByGuidAsync(clienteGuid);

        // Assert
        Assert.That(result.Nombre, Is.EqualTo(clienteResponse.Nombre));
        Assert.That(result.Guid, Is.EqualTo(clienteResponse.Guid));
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task GetByGuid_ClienteNotExist()
    {
        // Arrange
        _redisDatabaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.Cliente.Models.Cliente>.IsAny))
            .Returns(true);

        // Act
        var result = await _clienteService.GetByGuidAsync("non-existing-guid");

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Create()
    {
        
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        
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
    public async Task CreateAsync_ClienteConDniExistente()
    {
        // Arrange
        var clienteRequest = new ClienteRequest
        {
            UserGuid = "user-guid",
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

        // Mock de _userService.GetUserModelByGuid
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        
        var userE = new UserEntity
        {
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        _dbContext.Usuarios.Add(userE);
        await _dbContext.SaveChangesAsync();
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync(clienteRequest.UserGuid))
            .ReturnsAsync(user);

        // Simulamos que ya existe un cliente con ese DNI
        await _clienteService.CreateAsync(new ClienteRequest
        {
            UserGuid = "user-guid",
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
        });
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("DNI ya existe"));
    }
    
    [Test]
    public async Task Update()
    {
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        
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
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        
        var result = await _clienteService.UpdateAsync("non-existing-guid", new ClienteRequestUpdate());
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Update_WhenDniAlreadyExists()
    {
        var userEntity = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        

        var clienteToUpdate = new ClienteEntity
        {
            Guid = "update-guid-1",
            Dni = "12345678Z",  
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
                _clienteService.UpdateAsync("update-guid-2", updateRequest) 
        );

        Assert.That(ex?.Message, Is.EqualTo("Ya existe un cliente con el DNI: 12345678Z")); 
    }
    
    
    
    [Test]
    public async Task DeleteByGuid()
    {
        
        var userEntity  = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        _dbContext.Usuarios.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        
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
   
    [Test]
    public async Task GetAllForStorage_Empty()
    {
        // Act
        var result = await _clienteService.GetAllForStorage();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
    
    [Test]
    public async Task GetAllForStorage_Success()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        var user2 = new UserEntity{Id = 2,Guid = "user-guid2", Username = "user1", Password = "password",IsDeleted = false};
        _dbContext.Usuarios.AddRange(user1, user2);
        var cliente1 = new ClienteEntity { Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};
        var cliente2 = new ClienteEntity { Nombre = "Cliente 2", Guid = "guid", Dni = "12345678Q", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user2.Id};
        await _dbContext.Clientes.AddRangeAsync(cliente1, cliente2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _clienteService.GetAllForStorage();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That( result.Count(),Is.EqualTo(2));
        
    }
    
    [Test]
    public async Task DerechoAlOlvido_NotFoundUser()
    {
        // Arrange
        var userGuid = Guid.NewGuid().ToString();
        _userServiceMock
            .Setup(us => us.GetUserModelByGuidAsync(userGuid))
            .ReturnsAsync((Banco_VivesBank.User.Models.User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await _clienteService.DerechoAlOlvido(userGuid));
        Assert.That($"Usuario no encontrado con guid: {userGuid}", Is.EqualTo(ex.Message));
    }
    
    [Test]
    public async Task DerechoAlOlvido_Sucess()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        var cliente = new ClienteEntity { Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        
        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid", Id = 1 };

        _userServiceMock
            .Setup(us => us.GetUserModelByGuidAsync(cliente.Guid))
            .ReturnsAsync(user);

        // Act
        var result = await _clienteService.DerechoAlOlvido("userGuid");

        // Assert
        Assert.That("Datos del cliente eliminados de la base de datos", Is.EqualTo(result));
        var clienteEliminado = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Guid == cliente.Guid);
        Assert.That(clienteEliminado, Is.Not.Null);
        Assert.That(clienteEliminado.IsDeleted, Is.True);
        Assert.That(clienteEliminado.Dni, Is.Not.EqualTo(cliente.Dni));
    }
    
    [Test]
    public async Task UpdateFotoPerfil_ClienteNotFound()
    {
        // Arrange
        var guid = "algo";
        var fotoPerfil = new Mock<IFormFile>().Object;

        // Act
        var result = await _clienteService.UpdateFotoPerfil(guid, fotoPerfil);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateFotoPerfil_Success()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        var cliente = new ClienteEntity { Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var nuevaFotoUrl = "https://example.com/newFoto.jpg";
        _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(nuevaFotoUrl);

        // Act
        var result = await _clienteService.UpdateFotoPerfil(cliente.Guid, new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(nuevaFotoUrl, Is.EqualTo(result!.FotoPerfil));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(nuevaFotoUrl, Is.EqualTo(clienteActualizado.FotoPerfil));
    }
    
    [Test]
    public async Task UpdateFotoDni_ClienteNotFound()
    {
        // Arrange
        var guid = "ALGO";
        var fotoDni = new Mock<IFormFile>().Object;

        // Act
        var result = await _clienteService.UpdateFotoDni(guid, fotoDni);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateFotoDni_ClienteExiste_ActualizaFotoDni()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        var cliente = new ClienteEntity { Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var nuevaFotoDniUrl = "https://example.com/newFotoDni.jpg";
        _storageService
            .Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(nuevaFotoDniUrl);

        // Act
        var result = await _clienteService.UpdateFotoDni(cliente.Guid, new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(nuevaFotoDniUrl, Is.EqualTo(result!.FotoDni));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(nuevaFotoDniUrl, Is.EqualTo(clienteActualizado.FotoDni));
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