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
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Role = Banco_VivesBank.User.Models.Role;

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
    [Order(1)]
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
    [Order(37)]
    public async Task GetAllPagedAsync_Empty()
    {
        // Arrange
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 1,
            SortBy = "id",
            Direction = "ASC"
        };

        // Act
        var result = await _clienteService.GetAllPagedAsync(null, null, null, pageRequest);

        // Assert
        Assert.That(result.Content, Is.Empty);
        Assert.That(result.PageNumber, Is.EqualTo(0));
        Assert.That(result.PageSize, Is.EqualTo(1));
        Assert.That(result.TotalElements, Is.EqualTo(0));
        Assert.That(result.TotalPages, Is.EqualTo(0));
        Assert.That(result.Empty, Is.True);
        Assert.That(result.First, Is.True);
        Assert.That(result.Last, Is.False);
    }
    
    [Test]
    [Order(38)]
    public async Task GetAllPagedAsync_FiltradoPorNombre()
    {
        UserEntity user1 = new UserEntity { Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false };
        UserEntity user2 = new UserEntity { Guid = "user-guid2", Username = "user2", Password = "password", IsDeleted = false };
        await _dbContext.Usuarios.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        ClienteEntity cliente1 = new ClienteEntity
        {
            Guid = "a",
            Nombre = "test1",
            Dni = "a",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user1.Id
        };
        ClienteEntity cliente2 = new ClienteEntity
        {
            Guid = "b",
            Nombre = "test2",
            Dni = "b",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user2.Id
        };
        _dbContext.Clientes.AddRange(cliente1, cliente2);
        await _dbContext.SaveChangesAsync();
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "id",
            Direction = "ASC"
        };

        var result = await _clienteService.GetAllPagedAsync("test1", null, null, pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content.First().Nombre, Is.EqualTo("test1"));
    }

    [Test]
    [Order(39)]
    public async Task GetAllPagedAsync_FiltradoPorApellidos()
    {
        UserEntity user1 = new UserEntity { Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false };
        UserEntity user2 = new UserEntity { Guid = "user-guid2", Username = "user2", Password = "password", IsDeleted = false };
        await _dbContext.Usuarios.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        ClienteEntity cliente1 = new ClienteEntity
        {
            Guid = "a",
            Nombre = "test1",
            Dni = "a",
            Apellidos = "abc",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user1.Id
        };
        ClienteEntity cliente2 = new ClienteEntity
        {
            Guid = "b",
            Nombre = "test2",
            Dni = "b",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user2.Id
        };
        _dbContext.Clientes.AddRange(cliente1, cliente2);
        await _dbContext.SaveChangesAsync();
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "id",
            Direction = "ASC"
        };

        var result = await _clienteService.GetAllPagedAsync(null, "a", null, pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(2));
        Assert.That(result.Content.All(c => c.Apellidos.StartsWith("a")), Is.True);
    }

    [Test]
    [Order(40)]
    public async Task GetAllPagedAsync_FiltradoPorDni()
    {
        UserEntity user1 = new UserEntity { Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false };
        UserEntity user2 = new UserEntity { Guid = "user-guid2", Username = "user2", Password = "password", IsDeleted = false };
        await _dbContext.Usuarios.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        ClienteEntity cliente1 = new ClienteEntity
        {
            Guid = "a",
            Nombre = "test1",
            Dni = "a",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user1.Id
        };
        ClienteEntity cliente2 = new ClienteEntity
        {
            Guid = "b",
            Nombre = "test2",
            Dni = "b",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user2.Id
        };
        _dbContext.Clientes.AddRange(cliente1, cliente2);
        await _dbContext.SaveChangesAsync();
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "id",
            Direction = "ASC"
        };

        var result = await _clienteService.GetAllPagedAsync(null, null, "a", pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content.First().Dni, Is.EqualTo("a"));
    }

    [Test]
    [Order(41)]
    public async Task GetAllPagedAsync_Paginado()
    {
        UserEntity user1 = new UserEntity { Guid = "user-guid", Username = "user1", Password = "password", IsDeleted = false };
        UserEntity user2 = new UserEntity { Guid = "user-guid2", Username = "user2", Password = "password", IsDeleted = false };
        await _dbContext.Usuarios.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        ClienteEntity cliente1 = new ClienteEntity
        {
            Guid = "a",
            Nombre = "test1",
            Dni = "a",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user1.Id
        };
        ClienteEntity cliente2 = new ClienteEntity
        {
            Guid = "b",
            Nombre = "test2",
            Dni = "b",
            Apellidos = "a",
            Email = "example",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" },
            Telefono = "600000000",
            UserId = user2.Id
        };
        _dbContext.Clientes.AddRange(cliente1, cliente2);
        await _dbContext.SaveChangesAsync();
        var pageRequest = new PageRequest
        {
            PageNumber = 1, // Segunda página
            PageSize = 1,
            SortBy = "id",
            Direction = "ASC"
        };

        var result = await _clienteService.GetAllPagedAsync(null, null, null, pageRequest);

        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(1));
        Assert.That(result.TotalElements, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.Empty, Is.False);
        Assert.That(result.First, Is.False);
        Assert.That(result.Last, Is.True);
    }
    
    [Test]
    [Order(2)]
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
    [Order(3)]
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
    [Order(4)]
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
    [Order(5)]
    public async Task GetByGuid_ClienteNotFound()
    {
        var cacheKey = "Cliente:inexistente";
        _redisDatabase.Setup(x => x.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _clienteService.GetByGuidAsync("algo");

        Assert.That(result, Is.Null);
    }
    
    // [Test]
    // [Order(6)]
    // public async Task Create()
    // {
    //     var userEntity = new UserEntity
    //     {
    //         Id = 1,
    //         Guid = "user-guid",
    //         Username = "user1",
    //         Password = "password",
    //         Role = Role.User,
    //         IsDeleted = false,
    //         CreatedAt = DateTime.UtcNow,
    //         UpdatedAt = DateTime.UtcNow
    //     };
    //     
    //     _dbContext.Usuarios.Add(userEntity);
    //     await _dbContext.SaveChangesAsync();
    //     
    //     var clienteRequest = new ClienteRequest
    //     {
    //         Dni = "12345678Z",
    //         Nombre = "Juan",
    //         Apellidos = "Perez",
    //         Calle = "Calle Falsa",
    //         Numero = "123",
    //         CodigoPostal = "28000",
    //         Piso = "2",
    //         Letra = "A",
    //         Email = "juanperez@example.com",
    //         Telefono = "600000000",
    //         IsDeleted = false
    //     };
    //     
    //     var cacheKey = "Cliente:" + userEntity.Guid;
    //     
    //     _redisDatabase
    //         .Setup(db => db.StringSetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<RedisValue>(), TimeSpan.FromMinutes(30), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(true);
    //     
    //     var result = await _clienteService.CreateAsync(userEntity.ToModelFromEntity() , clienteRequest);
    //     
    //     Assert.That(result, Is.Not.Null);
    //     Assert.That(result.Dni, Is.EqualTo(clienteRequest.Dni));
    //     Assert.That(result.Email, Is.EqualTo(clienteRequest.Email));
    //     Assert.That(result.UserResponse.Role, Is.EqualTo(Role.Cliente.ToString()));
    //     
    //     
    //     _redisDatabase.Verify(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    // }

     [Test]
    [Order(7)]
     public async Task Create_UserYaAsociado()
     {
         var userEntity = new UserEntity
         {
             Guid = "user-guid",
             Username = "Test User",
             Password = "password",
             IsDeleted = false,
             Id = 1
         };
         var clienteEntity = new ClienteEntity
         {
             Id = 1,
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
             Email = "example",
             Telefono = "600000000",
             IsDeleted = false,
             UserId = 1
         };
            _dbContext.Usuarios.Add(userEntity);
            _dbContext.Clientes.Add(clienteEntity);
            await _dbContext.SaveChangesAsync();
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
         var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid" };
         var ex = Assert.ThrowsAsync<ClienteExistsException>(async () => await _clienteService.CreateAsync(user,  clienteRequest));
         Assert.That(ex.Message, Is.EqualTo($"El usuario con guid {user.Guid} ya es un cliente"));
     }
    
    [Test]
    [Order(8)]
    public async Task CreateAsync_ClienteConDniExistente()
    {
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
       
        var userEntityExistente = new UserEntity
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
        _dbContext.Usuarios.AddRange(userEntityExistente, userE2);
        await _dbContext.SaveChangesAsync();
        var clienteExistente = new ClienteEntity{ Dni = "12345678A" ,Guid = "cliente-guid", Nombre = "Juan", Apellidos = "Perez", Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Email = "ejemplo@gmail.com", Telefono = "600000000", IsDeleted = false, UserId = 1};
        _dbContext.Clientes.Add(clienteExistente);
        await _dbContext.SaveChangesAsync();
        
        var user = _dbContext.Usuarios.FirstOrDefault(u => u.Guid == userE2.Guid).ToModelFromEntity();
        
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el DNI: 12345678A"));
    }
    
     [Test]
     [Order(9)]
    public async Task CreateAsync_ClienteConTelefonoExistente()
    {
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
            Telefono = "600000000",
            IsDeleted = false
        };
       
        var userEntityExistente = new UserEntity
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
        _dbContext.Usuarios.AddRange(userEntityExistente, userE2);
        await _dbContext.SaveChangesAsync();
        var clienteExistente = new ClienteEntity{  Telefono = "600000000", Dni = "12345678B" ,Guid = "cliente-guid", Nombre = "Juan", Apellidos = "Perez", Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Email = "ejemplo@gmail.com", IsDeleted = false, UserId = 1};
        _dbContext.Clientes.Add(clienteExistente);
        await _dbContext.SaveChangesAsync();
        
        var user = _dbContext.Usuarios.FirstOrDefault(u => u.Guid == userE2.Guid).ToModelFromEntity();
        
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el teléfono: 600000000"));
    }
    
     [Test]
     [Order(10)]
    public async Task CreateAsync_ClienteConEmailExistente()
    {
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
            Telefono = "60000000",
            IsDeleted = false
        };
       
        var userEntityExistente = new UserEntity
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
        _dbContext.Usuarios.AddRange(userEntityExistente, userE2);
        await _dbContext.SaveChangesAsync();
        var clienteExistente = new ClienteEntity{  Email = "juanaperez@example.com", Telefono = "60000100", Dni = "12345678B" ,Guid = "cliente-guid", Nombre = "Juan", Apellidos = "Perez", Direccion = new Direccion { Calle = "Calle Falsa", Numero = "123", CodigoPostal = "28000", Piso = "2", Letra = "A" }, IsDeleted = false, UserId = 1};
        _dbContext.Clientes.Add(clienteExistente);
        await _dbContext.SaveChangesAsync();
        
        var user = _dbContext.Usuarios.FirstOrDefault(u => u.Guid == userE2.Guid).ToModelFromEntity();
        
        var ex = Assert.ThrowsAsync<ClienteExistsException>(() =>
            _clienteService.CreateAsync(user, clienteRequest) 
        );
        // Act & Assert
        Assert.That(ex.Message, Is.EqualTo("Ya existe un cliente con el email: juanaperez@example.com"));
        
    }
    
    [Test]
    [Order(11)]
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
    [Order(12)]
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
        
        var userAuth = new Banco_VivesBank.User.Models.User { Id = 1, Guid = "user-guid" };
        
        var result = await _clienteService.UpdateMeAsync(userAuth, new ClienteRequestUpdate());
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    [Order(13)]
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
    [Order(14)]
    public async Task DeleteByGuid_ClienteNotExist()
    {
        var result = await _clienteService.DeleteByGuidAsync("non-existing-guid");
        
        Assert.That(result, Is.Null);
    }
   
    [Test]
    [Order(15)]
    public async Task GetAllForStorage_Empty()
    {
        // Act
        var result = await _clienteService.GetAllForStorage();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
    
    [Test]
    [Order(16)]
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
    [Order(17)]
    public async Task DerechoAlOlvido_ClienteNotFound()
    {
        // Arrange
        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid" };
       

        // Act & Assert
        var result = await _clienteService.DerechoAlOlvido(user);
        Assert.That(result , Is.EqualTo(null));
    }
    
    [Test]
    [Order(18)]
    public async Task DerechoAlOlvido_Success()
    {
        // Arrange
        var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid" };
        await _dbContext.Usuarios.AddAsync(user1);
        await _dbContext.SaveChangesAsync();
        var cliente = new ClienteEntity { Nombre = "Cliente 1", Guid = "guid", Dni = "12345678Z", Apellidos = "Perez", Email = "example", Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, Telefono = "600000000", UserId = user1.Id};

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _clienteService.DerechoAlOlvido(user);

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
    [Order(19)]
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
    [Order(20)]
    public async Task GetClienteModelById_ClienteNotFound()
    {
        var result = await _clienteService.GetClienteModelById(1);
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    [Order(21)]
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
    [Order(22)]
    public async Task GetClienteModelByGuid_ClienteNotFound()
    {
        var result = await _clienteService.GetClienteModelByGuid("algo");
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    [Order(23)]
    public async Task UpdateFotoPerfilClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User { Guid = "user-guid" };
        var fotoPerfil = new Mock<IFormFile>().Object;
    
        var result = await _clienteService.UpdateFotoPerfil(user, fotoPerfil);
    
        Assert.That(result, Is.Null);
    }

    [Test]
    [Order(24)]
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
    
        var result = await _clienteService.UpdateFotoPerfil(user1.ToModelFromEntity() , new Mock<IFormFile>().Object);
    
        Assert.That(result, Is.Not.Null);
        Assert.That(nuevaFotoUrl, Is.EqualTo(result!.FotoPerfil));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(nuevaFotoUrl, Is.EqualTo(clienteActualizado.FotoPerfil));
    }

    [Test]
    [Order(25)]
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
            await _clienteService.UpdateFotoPerfil(user1.ToModelFromEntity(), fotoPerfil));
        Assert.That(ex.Message, Is.EqualTo("Error al guardar la foto: Error saving file"));
    }
    
    [Test]
    [Order(26)]
    public async Task UpdateFotoDniClienteNotFound()
    {
        // Arrange
        var user = new Banco_VivesBank.User.Models.User { Guid = "non-existing-guid" };
        var fotoDni = new Mock<IFormFile>().Object;
    
        // Act
        var result = await _clienteService.UpdateFotoDni(user, fotoDni);
    
        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    [Order(27)]
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
    
        var result = await _clienteService.UpdateFotoDni(user1.ToModelFromEntity(), new Mock<IFormFile>().Object);
    
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoDni, Is.EqualTo(nuevaFotoDniUrl));
        var clienteActualizado = await _dbContext.Clientes.FirstAsync(c => c.Guid == cliente.Guid);
        Assert.That(clienteActualizado.FotoDni, Is.EqualTo(nuevaFotoDniUrl));
    }
    
    [Test]
    [Order(28)]
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
            await _clienteService.UpdateFotoDni(user1.ToModelFromEntity(), fotoDni));
        Assert.That(ex.Message, Is.EqualTo("Error al subir la nueva foto al servidor FTP."));
    }
    
  
    [Test]
    [Order(29)]
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
    
        var result = await _clienteService.UpdateFotoPerfil(user1.ToModelFromEntity(), new Mock<IFormFile>().Object);
    
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoPerfil, Is.EqualTo(nuevaFotoUrl));
        _storageService.Verify(fs => fs.DeleteFileAsync("https://example.com/oldFoto.jpg"), Times.Once);
    }
    
    // [Test]
    // [Order(30)]
    // public async Task UpdateFotoPerfil_NullNewFile()
    // {
    //     // Arrange
    //     var user1 = new UserEntity{Id = 1, Guid = "user-guid", Username ="user1", Password = "password", IsDeleted = false};
    //     await _dbContext.Usuarios.AddAsync(user1);
    //     await _dbContext.SaveChangesAsync();
    //     
    //     var cliente = new ClienteEntity
    //     {
    //         Nombre = "Cliente 1", 
    //         Guid = "guid", 
    //         Dni = "12345678Z", 
    //         Apellidos = "Perez", 
    //         Email = "example", 
    //         FotoPerfil = "data/oldFoto.jpg",
    //         FotoDni = "data/oldDni.jpg",
    //         Direccion = new Direccion { Calle = "Calle", Numero = "1", CodigoPostal = "28000", Piso = "2", Letra = "A" }, 
    //         Telefono = "600000000", 
    //         UserId = user1.Id,
    //     };
    //     
    //     await _dbContext.Clientes.AddAsync(cliente);
    //     await _dbContext.SaveChangesAsync();
    //
    //     _storageService.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>())).ReturnsAsync((string)null);
    //
    //     // Act & Assert
    //     Assert.ThrowsAsync<FileStorageException>(async () => 
    //         await _clienteService.UpdateFotoPerfil(user1.ToModelFromEntity(), new Mock<IFormFile>().Object));
    // }
    
    [Test]
    [Order(31)]
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
        var result = await _clienteService.UpdateFotoDni(user1.ToModelFromEntity(), new Mock<IFormFile>().Object);
    
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FotoDni, Is.EqualTo($"data/{cliente.Dni}"));
    }
    
    [Test]
    [Order(32)]
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
    [Order(33)]
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

    [Test]
    [Order(34)]
    public async Task DeletMeAsync_ClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User { Guid = "non-existing-guid" };
        var result = await _clienteService.DeleteMeAsync(user);
        
        Assert.That(result, Is.Null);
    }

    [Test]
    [Order(35)]
    public async Task DeleteMeAsync()
    {
        UserEntity user = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        ClienteEntity cliente = new ClienteEntity
        {
            Id = 1,
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
            Email = "ejemplo@ejemplo.com",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };
        _dbContext.Usuarios.Add(user);
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var result = await _clienteService.DeleteMeAsync(user.ToModelFromEntity());
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDeleted, Is.True);
    }
    
    [Test]
    [Order(36)]
    public async Task GetMeAsync_ClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User { Guid = "non-existing-guid" };
        var result = await _clienteService.GetMeAsync(user);
        
        Assert.That(result, Is.Null);
    }

    [Test]
    [Order(37)]
    public async Task GetMeAsync()
    {
        UserEntity user = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "user1",
            Password = "password",
            IsDeleted = false
        };
        ClienteEntity cliente = new ClienteEntity
        {
            Id = 1,
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
            Email = "ejemplo",
            Telefono = "600000000",
            IsDeleted = false,
            UserId = 1
        };
        _dbContext.Usuarios.Add(user);
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var result = await _clienteService.GetMeAsync(user.ToModelFromEntity());

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(cliente.Nombre));
        Assert.That(result.Apellidos, Is.EqualTo(cliente.Apellidos));
        Assert.That(result.Dni, Is.EqualTo(cliente.Dni));
        Assert.That(result.Direccion.Calle, Is.EqualTo(cliente.Direccion.Calle));
        Assert.That(result.Email, Is.EqualTo(cliente.Email));
        Assert.That(result.Telefono, Is.EqualTo(cliente.Telefono));
        Assert.That(result.Guid, Is.EqualTo(cliente.Guid));
    }
}