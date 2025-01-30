using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Storage.Ftp.Service;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Images.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
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
    private Mock<IFtpService> _ftpService;
    private Mock<IUserService> _userServiceMock;
    private Mock<IFileStorageService> _storageService;
    
    private Mock<IConnectionMultiplexer> _redis;
    private Mock<IDatabase> _redisDatabase;

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

        _userServiceMock = new Mock<IUserService>();
        _storageService = new Mock<IFileStorageService>();
        _ftpService = new Mock<IFtpService>();

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _redisDatabase = new Mock<IDatabase>();
        _redis = new Mock<IConnectionMultiplexer>();
        _redis
            .Setup(conn => conn.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_redisDatabase.Object);
        
        _clienteService = new ClienteService(
            _dbContext, 
            NullLogger<ClienteService>.Instance,
            _userServiceMock.Object,
            _storageService.Object,
            _memoryCache,
            _redis.Object,
            _ftpService.Object
            );
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

        if (_memoryCache != null)
        {
            _memoryCache.Dispose();
        }
    }  
    
 
    [Test]
    public async Task GetAllPagedAsync()
    {
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 1,
            SortBy = "id",
            Direction = "ASC"
        };
        var user1 = new UserEntity { Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false };
        var user2 = new UserEntity { Guid = "user-guid2", Username = "user2", Password = "password", IsDeleted = false };
        await _dbContext.Usuarios.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Clientes.AddRangeAsync(new ClienteEntity
            {
                Nombre = "test1", Guid = "a", Dni = "a", Apellidos = "a", Email = "example",
                Direccion = new Direccion
                    { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
                Telefono = "600000000", UserId = user1.Id
            },
            new ClienteEntity
            {
                Guid = "b", Nombre = "test2", Dni = "b", Apellidos = "a", Email = "example",
                Direccion = new Direccion
                    { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
                Telefono = "600000000", UserId = user2.Id
            });
        await _dbContext.SaveChangesAsync();

        var result = await _clienteService.GetAllPagedAsync(null, null, null, pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(0));
        Assert.That(result.PageSize, Is.EqualTo(1));
        Assert.That(result.TotalElements, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.Empty, Is.False);
        Assert.That(result.First, Is.True);
        Assert.That(result.Last, Is.False);
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
        var cacheKey ="Cliente:" + cliente.Guid;
        
        _userServiceMock.Setup(x => x.GetUserModelByIdAsync(cliente.UserId)).ReturnsAsync(user);
        
        _redisDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);
        
        var result = await _clienteService.GetByGuidAsync(cliente.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cliente.Guid));
    }
    
    [Test]
    public async Task GetByGuid_ClienteEnCacheMemoria()
    {
        var clienteGuid = "existing-guid";
        var cacheKey = $"Cliente:{clienteGuid}";
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = clienteGuid,
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
            User = new Banco_VivesBank.User.Models.User
            {
                Id = 1, Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false
            }
        };
        var clienteResponse = new ClienteResponse
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente"
        };
            
        cacheKey = $"Cliente:{clienteGuid}";
        _memoryCache.Set(cacheKey, cliente);
        
    
        var result = await _clienteService.GetByGuidAsync(clienteGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cliente.Guid));
        Assert.That(result.Nombre, Is.EqualTo(cliente.Nombre));
        Assert.That(result.Apellidos, Is.EqualTo(cliente.Apellidos));
        Assert.That(result.Dni, Is.EqualTo(cliente.Dni));
        Assert.That(result.Direccion.Calle, Is.EqualTo(cliente.Direccion.Calle));
        Assert.That(result.Direccion.Numero, Is.EqualTo(cliente.Direccion.Numero));
        Assert.That(result.Direccion.CodigoPostal, Is.EqualTo(cliente.Direccion.CodigoPostal));
        Assert.That(result.Direccion.Piso, Is.EqualTo(cliente.Direccion.Piso));

    }

    [Test]
    public async Task GetByGuid_ClienteEnRedis()
    {
        // Arrange
        var clienteGuid = "existing-guid2";
        var clienteResponse = new ClienteResponse
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente",
            Email = "test@example.com",
            Dni = "12345678A",
            Telefono = "123456789",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            IsDeleted = false
        };

        var clienteModel = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente",
            Email = "asdas",
            Dni = "12345678A",
            Telefono = "123456789",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            User = new Banco_VivesBank.User.Models.User {
                Id = 1,
                Guid = "user-guid",
                Username = "user1",
                Password = "password",
                IsDeleted = false},
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
        var clienteEntity = new ClienteEntity
        {
            Guid = clienteGuid,
            Nombre = "Test Cliente",
            Apellidos = "TEST",
            Email = "asdas",
            Dni = "12345678A",
            Telefono = "123456789",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                CodigoPostal = "28000",
                Piso = "2",
                Letra = "A"
            },
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userEntity.Id,
            User = userEntity
        };
        _dbContext.Clientes.Add(clienteEntity);
        await _dbContext.SaveChangesAsync();

        var cacheKey = $"Cliente:{clienteGuid}";
        _memoryCache.Remove(cacheKey);
        var redisValue = JsonSerializer.Serialize(clienteModel);

        // Mock de Redis
         _redisDatabase.Setup(db => db.StringGetAsync(It.Is<RedisKey>(k => k==cacheKey), It.IsAny<CommandFlags>())).ReturnsAsync(redisValue);
         
        
        // Act
        var result = await _clienteService.GetByGuidAsync(clienteGuid);

        // Assert
        Assert.That(result.Nombre, Is.EqualTo(clienteResponse.Nombre));
        Assert.That(result.Guid, Is.EqualTo(clienteResponse.Guid));
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task GetByGuid_ClienteNotFound()
    {
        var cacheKey = "Cliente:inexistente";
        _redisDatabase.Setup(x => x.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _clienteService.GetByGuidAsync("algo");

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
        
        var cacheKey = "Cliente:" + userEntity.Guid;
        
        _redisDatabase
            .Setup(db => db.StringSetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<RedisValue>(), TimeSpan.FromMinutes(30), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
       
        
        var result = await _clienteService.CreateAsync(user , clienteRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Dni, Is.EqualTo(clienteRequest.Dni));
        Assert.That(result.Email, Is.EqualTo(clienteRequest.Email));
        
        _redisDatabase.Verify(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    // [Test] Cambiar para que user ya este asociado a un cliente
    // public async Task Create_UserNotFound()
    // {
    //     var user = new Banco_VivesBank.User.Models.User
    //     {
    //         Guid = "user-guid",
    //         Username = "Test User",
    //         Password = "password",
    //         IsDeleted = false,
    //         Id = 1
    //     };
    //     var user
    //     var clienteRequest = new ClienteRequest
    //     {
    //         Dni = "12345678A",
    //         Nombre = "Juana",
    //         Apellidos = "Perez",
    //         Calle = "Calle Falsa",
    //         Numero = "123",
    //         CodigoPostal = "28000",
    //         Piso = "2",
    //         Letra = "A",
    //         Email = "juanaperez@example.com",
    //         Telefono = "600100000",
    //         IsDeleted = false
    //     };
    //     
    //     var ex = Assert.ThrowsAsync<UserNotFoundException>(async () => await _clienteService.CreateAsync( user , clienteRequest));
    //     Assert.That(ex.Message, Is.EqualTo($"Usuario no encontrado con guid: {user.Guid}"));
    // }
    
    [Test]
    public async Task CreateAsync_ClienteConDniExistente()
    {
        CleanDatabase();
        // Arrange
        var clienteRequest = new ClienteRequest
        {
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
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var user2 = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        
        var userE = new UserEntity
        {
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var userE2 = new UserEntity
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        _dbContext.Usuarios.AddRange(userE, userE2);
        await _dbContext.SaveChangesAsync();
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid"))
            .ReturnsAsync(user);
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid2"))
            .ReturnsAsync(user2);

        // Simulamos que ya existe un cliente con ese DNI
        await _clienteService.CreateAsync(user2, new ClienteRequest
        {
            
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
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el DNI: 12345678A"));
        
    }
    
     [Test]
    public async Task CreateAsync_ClienteConTelefonoExistente()
    {
        // Arrange
        var clienteRequest = new ClienteRequest
        {
            Dni = "12345678S",
            Nombre = "Juana",
            Apellidos = "Perez",
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A",
            Email = "juanaperez@example.com",
            Telefono = "600000000",
            IsDeleted = false
        };

        // Mock de _userService.GetUserModelByGuid
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var user2 = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        
        var userE = new UserEntity
        {
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var userE2 = new UserEntity
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        _dbContext.Usuarios.AddRange(userE, userE2);
        await _dbContext.SaveChangesAsync();
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid"))
            .ReturnsAsync(user);
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid2"))
            .ReturnsAsync(user2);

        // Simulamos que ya existe un cliente con ese DNI
        await _clienteService.CreateAsync(user2, new ClienteRequest
        {
            Dni = "12345678A",
            Nombre = "Juana",
            Apellidos = "Perez",
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A",
            Email = "juanapereza@example.com",
            Telefono = "600000000",
            IsDeleted = false
        });
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el teléfono: 600000000"));
        
    }
    
     [Test]
    public async Task CreateAsync_ClienteConEmailExistente()
    {
        // Arrange
        var clienteRequest = new ClienteRequest
        {
            Dni = "12345678B",
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
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var user2 = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        
        var userE = new UserEntity
        {
            Guid = "user-guid",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 1
        };
        var userE2 = new UserEntity
        {
            Guid = "user-guid2",
            Username = "Test User",
            Password = "password",
            IsDeleted = false,
            Id = 2
        };
        _dbContext.Usuarios.AddRange(userE, userE2);
        await _dbContext.SaveChangesAsync();
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid"))
            .ReturnsAsync(user);
        _userServiceMock
            .Setup(u => u.GetUserModelByGuidAsync("user-guid2"))
            .ReturnsAsync(user2);

        // Simulamos que ya existe un cliente con ese DNI
        await _clienteService.CreateAsync(user2, new ClienteRequest
        {
            Dni = "12345678A",
            Nombre = "Juana",
            Apellidos = "Perez",
            Calle = "Calle Falsa",
            Numero = "123",
            CodigoPostal = "28000",
            Piso = "2",
            Letra = "A",
            Email = "juanaperez@example.com",
            Telefono = "600120000",
            IsDeleted = false
        });
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el email: juanaperez@example.com"));
        
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
        
        var result = await _clienteService.UpdateMeAsync(user, updateRequest);
        
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
        
        var userAuth = new User
        {
            Id = 1, 
            Guid = "user-guid"
        };
        
        var result = await _clienteService.UpdateMeAsync(userAuth, new ClienteRequestUpdate());
        
        Assert.That(result, Is.Null);
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
    public async Task DerechoAlOlvido_Success()
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
        var result = await _clienteService.DerechoAlOlvido("user-guid");

        // Assert
        Assert.That("Datos del cliente eliminados de la base de datos", Is.EqualTo(result));
        var clienteEliminado = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Guid == cliente.Guid);
        Assert.That(clienteEliminado, Is.Not.Null);
        Assert.That(clienteEliminado.IsDeleted, Is.True);
        Assert.That(clienteEliminado.Dni, Is.EqualTo(""));
        Assert.That(clienteEliminado.Email, Is.EqualTo(""));
        Assert.That(clienteEliminado.Telefono, Is.EqualTo(""));
    }

    [Test]
    public async Task GetClienteModelById()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        var cliente = new ClienteEntity {Id = 0, Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};
        await CleanDatabase();
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        
        var result = await _clienteService.GetClienteModelById(cliente.Id);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(cliente.Nombre));
        Assert.That(result.Apellidos, Is.EqualTo(cliente.Apellidos));
        Assert.That(result.Dni, Is.EqualTo(cliente.Dni));
        Assert.That(result.Direccion.Calle, Is.EqualTo(cliente.Direccion.Calle));
    }
    
    [Test]
    public async Task GetClienteModelById_ClienteNotFound()
    {
        var result = await _clienteService.GetClienteModelById(1);
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetClienteModelByGuid()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        var cliente = new ClienteEntity {Id = 0, Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        
        var result = await _clienteService.GetClienteModelByGuid(cliente.Guid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(cliente.Nombre));
        Assert.That(result.Apellidos, Is.EqualTo(cliente.Apellidos));
        Assert.That(result.Dni, Is.EqualTo(cliente.Dni));
        Assert.That(result.Direccion.Calle, Is.EqualTo(cliente.Direccion.Calle));
    }
    
    [Test]
    public async Task GetClienteModelByGuid_ClienteNotFound()
    {
        var result = await _clienteService.GetClienteModelByGuid("algo");
        
        Assert.That(result, Is.Null);
    }
    
    
    
    
    [Test]
    public async Task UpdateFotoPerfilClienteNotFound()
    {
        var guid = "non-existing-guid";
        var fotoPerfil = new Mock<IFormFile>().Object;

        var result = await _clienteService.UpdateFotoPerfil(guid, fotoPerfil);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateFotoPerfil()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id
        };
        
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var nuevaFotoUrl = "https://example.com/newFoto.jpg";
        _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>())).ReturnsAsync(nuevaFotoUrl);

        var result = await _clienteService.UpdateFotoPerfil(cliente.Guid, new Mock<IFormFile>().Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(nuevaFotoUrl, Is.EqualTo(result!.FotoPerfil));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(nuevaFotoUrl, Is.EqualTo(clienteActualizado.FotoPerfil));
    }

    [Test]
    public async Task UpdateFotoPerfilFileStorageException()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id
        };
        
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var fotoPerfil = new Mock<IFormFile>().Object;
        _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>())).ThrowsAsync(new FileStorageException("Error saving file"));

        var ex = Assert.ThrowsAsync<FileStorageException>(async () =>
            await _clienteService.UpdateFotoPerfil(cliente.Guid, fotoPerfil));
        Assert.That(ex.Message, Is.EqualTo("Error al guardar la foto: Error saving file"));
    }

    [Test]
    public async Task UpdateFotoDniClienteNotFound()
    {
        // Arrange
        var guid = "non-existing-guid";
        var fotoDni = new Mock<IFormFile>().Object;

        // Act
        var result = await _clienteService.UpdateFotoDni(guid, fotoDni);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateFotoDni()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id
        };

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var nuevaFotoDniUrl = "data/12345678Z";
        _ftpService.Setup(fs => fs.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _clienteService.UpdateFotoDni(cliente.Guid, new Mock<IFormFile>().Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoDni, Is.EqualTo(nuevaFotoDniUrl));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(clienteActualizado.FotoDni, Is.EqualTo(nuevaFotoDniUrl));
    }

    [Test]
    public async Task UpdateFotoDniException()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id
        };
        
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var fotoDni = new Mock<IFormFile>().Object;
        _ftpService.Setup(fs => fs.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("FTP error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _clienteService.UpdateFotoDni(cliente.Guid, fotoDni));
        Assert.That(ex.Message, Is.EqualTo("Error al subir la nueva foto al servidor FTP."));
    }
    
    
    
    
    [Test]
    public async Task UpdateFotoPerfil_DeleteOldFileFailure()
    {
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
    
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id,
            FotoPerfil = "https://example.com/oldFoto.jpg"
        };
    
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
    
        var nuevaFotoUrl = "https://example.com/newFoto.jpg";
        _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>())).ReturnsAsync(nuevaFotoUrl);
        _storageService.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>())).ThrowsAsync(new FileStorageNotFoundException("Archivo no encontrado"));

        var result = await _clienteService.UpdateFotoPerfil(cliente.Guid, new Mock<IFormFile>().Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoPerfil, Is.EqualTo(nuevaFotoUrl));
        _storageService.Verify(fs => fs.DeleteFileAsync("https://example.com/oldFoto.jpg"), Times.Once);
    }

    [Test]
    public async Task UpdateFotoPerfil_NullNewFile()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id
        };
        
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>())).ReturnsAsync((string)null);

        // Act & Assert
        Assert.ThrowsAsync<FileStorageException>(async () => 
            await _clienteService.UpdateFotoPerfil(cliente.Guid, new Mock<IFormFile>().Object));
    }

    [Test]
    public async Task UpdateFotoDni_DeleteOldFileFails()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id,
            FotoDni = "data/oldDni.jpg"
        };

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        _ftpService.Setup(fs => fs.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _ftpService.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _clienteService.UpdateFotoDni(cliente.Guid, new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoDni, Is.EqualTo($"data/{cliente.Dni}"));
    }

    [Test]
    public async Task GetFotoDniAsync_FileNotFound()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        
        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1", 
            Guid = "guid", 
            Dni = "12345678Z", 
            Apellidos = "Perez", 
            Email = "example", 
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
            Telefono = "600000000", 
            UserId = user1.Id,
            FotoDni = "data/testDni.jpg"
        };

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        _ftpService.Setup(fs => fs.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Download failed"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => 
            await _clienteService.GetFotoDniAsync(cliente.Guid));
    }
    
    [Test]
    public async Task GetFotoDniAsync_ClienteConFotoDniValido()
    {
        var user1 = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();

        var cliente = new ClienteEntity
        {
            Nombre = "Cliente 1",
            Guid = "guid",
            Dni = "12345678Z",
            Apellidos = "Perez",
            Email = "example",
            Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user1.Id,
            FotoDni = "ftp://example.com/fotoDni.jpg"
        };
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();

        var tempFilePath = Path.GetTempFileName();
        _ftpService
            .Setup(ftp => ftp.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((url, path) =>
            {
                File.WriteAllText(path, "Contenido de prueba");
            })
            .Returns(Task.CompletedTask);

        var result = await _clienteService.GetFotoDniAsync(cliente.Guid);

        Assert.That(result, Is.Not.Null);
        using (var reader = new StreamReader(result))
        {
            var content = await reader.ReadToEndAsync();
            Assert.That(content, Is.EqualTo("Contenido de prueba"));
        }

        _ftpService.Verify(ftp => ftp.DownloadFileAsync(cliente.FotoDni, It.IsAny<string>()), Times.Once);
    }

}