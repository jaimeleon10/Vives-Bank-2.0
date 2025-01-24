/*using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace Banco_VivesBank.Test.User.Services;

[TestFixture]
public class UserServiceTest
{
    
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private UserService _userService;
    private Mock <IConnectionMultiplexer> _connectionMultiplexerMock;
    private Mock <ILogger> _loggerMock;
    private Mock <IDatabase> _databaseMock;
    private Mock <IMemoryCache> _memoryCacheMock;
    
    
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
        
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _loggerMock = new Mock<ILogger>();
        _databaseMock = new Mock<IDatabase>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        
        _userService = new UserService(
            _dbContext,
            NullLogger<UserService>.Instance,
            _connectionMultiplexerMock.Object,
            _memoryCacheMock.Object
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
        var user1 = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username1",
            Password = "password1",
            Role =Banco_VivesBank.User.Models.Role.User ,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
           
        };
        var user2 = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username2",
            Password = "password2",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();

        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "Username",
            Direction = "ASC"
        };

        var result = await _userService.GetAllAsync("username1",null, pageRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content[0].Username, Is.EqualTo(user1.Username));
        
    }
    
    [Test]
    public async Task GetByGuid()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "guid",
            Username = "username",
            Role =Banco_VivesBank.User.Models.Role.User,
        };

        await _dbContext.Usuarios.AddAsync(UserMapper.ToEntityFromModel(user));
        await _dbContext.SaveChangesAsync();

        var result = await _userService.GetByGuidAsync("guid");

        Assert.That(result.Guid, Is.EqualTo(user.Guid));
    }
    
    
    [Test]
    public async Task GetByGuid_NotFound()
    {
        var result = await _userService.GetByGuidAsync("nonexistent-guid");

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Create()
    {
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        var result = await _userService.CreateAsync(userRequest);

        Assert.That(result.Username, Is.EqualTo(userRequest.Username));
        Assert.That(result.Role, Is.EqualTo("USER"));
    }
    
    
    [Test]
    public async Task Update()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "guid",
            Username = "username",
            Role = Banco_VivesBank.User.Models.Role.User,
        };

        await _dbContext.Usuarios.AddAsync(UserMapper.ToEntityFromModel(user));
        await _dbContext.SaveChangesAsync();

        var userRequest = new UserRequest
        {
            Username = "username",
            Role = "USER",
        };

        var result = await _userService.UpdateAsync("guid", userRequest);

        Assert.That(result.Username, Is.EqualTo(userRequest.Username));
        Assert.That(result.Role, Is.EqualTo("USER"));
        Assert.That(result.Guid, Is.EqualTo(user.Guid));
        
    }
    
    [Test]
    public async Task Update_NotFound()
    {
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        var result = await _userService.UpdateAsync("nonexistent-guid", userRequest);

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Delete()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "guid",
            Username = "username",
            IsDeleted = false,
        };

        await _dbContext.Usuarios.AddAsync(UserMapper.ToEntityFromModel(user));
        await _dbContext.SaveChangesAsync();

        var result = await _userService.DeleteByGuidAsync("guid");

        Assert.That(result.Guid, Is.EqualTo(user.Guid));
        Assert.That(result.IsDeleted, Is.True);
    }

    [Test]
    public async Task Delete_NotFound()
    {
        var result = await _userService.DeleteByGuidAsync("nonexistent-guid");

        Assert.That(result, Is.Null);

    }

}
*/

