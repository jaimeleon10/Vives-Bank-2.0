using Banco_VivesBank.Database;
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
    private UserRequest _userRequest;
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

        _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _connectionMultiplexerMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

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
   /* [Test]
    public async Task GetByGuidAsync_Success()
    {
        var userGuid = "user-guid";
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = userGuid,
            Username = "username",
            Role = Banco_VivesBank.User.Models.Role.User
        };
        
        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny))
            .Returns((object key, out Banco_VivesBank.User.Models.User? cachedValue) =>
            {
                cachedValue = user;
                return true;
            });

        var result = await _userService.GetByGuidAsync(userGuid);

        // Verificaciones
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(userGuid));
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Role, Is.EqualTo(user.Role));

        _memoryCacheMock.Verify(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }
    */
   /* [Test]
    public async Task GetByGuidAsync_NotFound()
    {
        // Arrange
        var userGuid = "user-guid";

        // Configurar mocks
        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny))
            .Returns((object key, out Banco_VivesBank.User.Models.User? cachedValue) =>
            {
                cachedValue = null;
                return false;
            });

        var result = await _userService.GetByGuidAsync(userGuid);

        // Verificaciones
        Assert.That(result, Is.Null);

        _memoryCacheMock.Verify(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }
    */


    [Test]
    public async Task GetByUsername_Succesfully()
    {
        var user = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username",
            Password = "password",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();

        var result = await _userService.GetByUsernameAsync(user.Username);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(user.Username));
        
    }
    
   /* [Test]
    public async Task CreateAsync_ShouldCreateUserSuccessfully()
    {
        var userRequest = new UserRequest
        {
            Username = "newuser",
            Password = "password",
            Role = "User",
            IsDeleted = false
        };

        _databaseMock.Setup(db => db.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>())).ReturnsAsync(true);

        var result = await _userService.CreateAsync(userRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(userRequest.Username));
        Assert.That(result.Role, Is.EqualTo("User"));
    
        var userEntity = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Username == userRequest.Username);
        Assert.That(userEntity, Is.Not.Null);
        Assert.That(userEntity.Username, Is.EqualTo(userRequest.Username));

        var cacheKey = "User:" + userRequest.Username.ToLower();
        _databaseMock.Verify(db => db.StringSetAsync(cacheKey, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateUserSuccessfully()
    {
        var user = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username",
            Password = "password",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();

        var userRequest = new UserRequest
        {
            Username = "newusername",
            Password = "newpassword",
            Role = "User",
        };

        var result = await _userService.UpdateAsync(user.Guid, userRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(userRequest.Username));
        Assert.That(result.Role, Is.EqualTo("User"));

        var updatedUser = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Guid == user.Guid);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser.Username, Is.EqualTo(userRequest.Username));
    }
    */


    [Test]
    public async Task Update_NotFound()
    {
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "User",
        };

        var result = await _userService.UpdateAsync("nonexistent-guid", userRequest);

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Delete()
    {
        var user = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username",
            Password = "password",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();

        await _userService.DeleteByGuidAsync(user.Guid);

        var deletedUser = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Guid == user.Guid);
        Assert.That(deletedUser, Is.Not.Null);
        Assert.That(deletedUser.IsDeleted, Is.True);
    }

    [Test]
    public async Task Delete_NotFound()
    {
        var result = await _userService.DeleteByGuidAsync("nonexistent-guid");

        Assert.That(result, Is.Null);

    }

}


