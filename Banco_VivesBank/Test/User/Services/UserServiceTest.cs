using System.Security.Claims;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.ProductoBase.Mappers;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Auth.Jwt;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace Test.User.Services;

[TestFixture]
public class UserServiceTest
{
    
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private UserService _userService;
    private Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private Mock<ILogger<UserService>> _loggerMock;
    private Mock<IDatabase> _databaseMock;
    private Mock<IMemoryCache> _memoryCacheMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;

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
        _loggerMock = new Mock<ILogger<UserService>>();
        _databaseMock = new Mock<IDatabase>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _jwtServiceMock = new Mock<IJwtService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _connectionMultiplexerMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _userService = new UserService(
            _dbContext,
            _loggerMock.Object,
            _connectionMultiplexerMock.Object,
            _memoryCacheMock.Object,
            _jwtServiceMock.Object,
            _httpContextAccessorMock.Object
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

   /* [Test]
    public async Task GetAll()
    {
        var user1 = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "username1",
            Password = "password1",
            Role = Banco_VivesBank.User.Models.Role.User,
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

        var result = await _userService.GetAllAsync("username1", null, pageRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content[0].Username, Is.EqualTo(user1.Username));
    }
    */
    
    
    
   [Test]
    public async Task GetAllReturnsEmptyList()
    {
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "Username",
            Direction = "ASC"
        };
        var result = await _userService.GetAllAsync("nonexistent_user", null, pageRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.Empty);
    }



    [Test]
    public async Task GetByIdAsync_Success()
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

        var result = await _userService.GetByGuidAsync(user.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(user.Guid));
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Role, Is.EqualTo(user.Role.ToString()));
        
    }

   /* [Test]
    public async Task GetByIdAsync_FromRedis()
    {
        var userGuid = "user-guid";
        var userEntity = new UserEntity
        {
            Guid = userGuid,
            Username = "username",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny))
            .Returns((object key, out Banco_VivesBank.User.Models.User? cachedValue) =>
            {
                cachedValue = new Banco_VivesBank.User.Models.User
                {
                    Guid = userGuid,
                    Username = userEntity.Username,
                    Role = Role.User
                };
                return true;
            });

        var result = await _userService.GetByGuidAsync(userGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(userGuid));
        Assert.That(result.Username, Is.EqualTo(userEntity.Username));
        Assert.That(result.Role, Is.EqualTo(userEntity.Role.ToString()));

        _memoryCacheMock.Verify(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<Banco_VivesBank.User.Models.User>.IsAny), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        
    }
    */


   [Test]
   public async Task GetByGuidAsync_FromDatabase()
   {
       var userGuid = "user-guid";
       var userEntity = new UserEntity
       {
           Guid = userGuid,
           Username = "username",
           Password = "password",
           Role = Banco_VivesBank.User.Models.Role.User,
           IsDeleted = false,
           CreatedAt = DateTime.UtcNow,
           UpdatedAt = DateTime.UtcNow
       };
       _dbContext.Usuarios.Add(userEntity);
       await _dbContext.SaveChangesAsync();

       var result = await _userService.GetByGuidAsync(userGuid);

       Assert.That(result, Is.Not.Null);
       Assert.That(result.Guid, Is.EqualTo(userGuid));
       Assert.That(result.Username, Is.EqualTo(userEntity.Username));
       Assert.That(result.Role, Is.EqualTo(userEntity.Role.ToString()));
   }

   
    [Test]
    public async Task GetByGuidAsync_NotFound()
    {
        var result = await _userService.GetByGuidAsync("nonexistent-guid");

        Assert.That(result, Is.Null);
    }
    


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
    
    
    [Test]
    public async Task GetByUsername_NotFound()
    {
        var result = await _userService.GetByUsernameAsync("nonexistent-username");

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserModelByGuidAsync_Successfully()
    {
        
        var user = new UserEntity
        {
            Guid = "test-guid",
            Username = "username1",
            Password = "password1",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var result = await _userService.GetUserModelByGuidAsync("test-guid");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(user.Guid));
        Assert.That(result.Username, Is.EqualTo(user.Username));
    }
    
    [Test]
    public async Task GetUserModelByGuidAsync_NotFound()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        await _dbContext.SaveChangesAsync();
        
        var result = await _userService.GetUserModelByGuidAsync("non-existent-guid");
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetUserModelByIdAsync_UserFound()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        await _dbContext.SaveChangesAsync();
        
        var user = new UserEntity
        {
            Id = 1, 
            Guid = "test-guid",
            Username = "username1",
            Password = "password1",
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var result = await _userService.GetUserModelByIdAsync(1);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(user.Id));
        Assert.That(result.Username, Is.EqualTo(user.Username));
    }

    
    [Test]
    public async Task GetUserModelByIdAsync_UserNotFound()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        await _dbContext.SaveChangesAsync();
        
        var result = await _userService.GetUserModelByIdAsync(999);
        
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task CreateAsync_Successfully()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        await _dbContext.SaveChangesAsync();
        
        var userRequest = new UserRequest
        {
            Username = "username1",
            Password = "password1",
            PasswordConfirmation = "password1",
            
        };
        
        var result = await _userService.CreateAsync(userRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(userRequest.Username));
    }
    
    [Test]
    public async Task CreateAsync_UserAlreadyExists()
    {
        var user = new UserEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Username = "newuser",
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
            Username = "newuser",
            Password = "password",
        };

        Assert.ThrowsAsync<UserExistException>(() => _userService.CreateAsync(userRequest));
    }
    [Test]
    public void Authenticate_Successfully()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        _dbContext.SaveChanges();
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password1");
        var user = new UserEntity
        {
            Guid = "test-guid",
            Username = "username1",
            Password = hashedPassword,
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.Add(user);
        _dbContext.SaveChanges();
        
        var addedUser = _dbContext.Usuarios.FirstOrDefault(u => u.Username == "username1");
        Assert.That(addedUser, Is.Not.Null);
        Assert.That(addedUser.Password, Is.EqualTo(hashedPassword));
        
        var token = _userService.Authenticate("username1", "password1");
        
        Assert.That(token, Is.Null);
    }

    [Test]
    public void Authenticate_NotFound()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        _dbContext.SaveChanges();
        
        Assert.Throws<UnauthorizedAccessException>(() => _userService.Authenticate("non-existent-username", "password1"));
    }
    [Test]
    public void Authenticate_InvalidPassword()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password1");
        var user = new UserEntity
        {
            Guid = "test-guid",
            Username = "username1",
            Password = hashedPassword,
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.Add(user);
        _dbContext.SaveChanges();
        
        Assert.Throws<UnauthorizedAccessException>(() => _userService.Authenticate("username1", "wrong-password"));
    }
    
    [Test]
    public async Task UpdatePasswordAsync_NoPasswordMatch()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "test-guid",
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };

        var updatePasswordRequest = new UpdatePasswordRequest
        {
            OldPassword = "oldpassword",
            NewPassword = "newpassword",
            NewPasswordConfirmation = "differentpassword"
        };
        
        Assert.ThrowsAsync<InvalidPasswordException>(() => _userService.UpdatePasswordAsync(user, updatePasswordRequest));
    }
   [Test]
   public async Task UpdateAsyncSuccesfully()
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
       var userRequestUpdate = new UserRequestUpdate
       {
           Role = "User",
           IsDeleted = false
       };

       var result = await _userService.UpdateAsync(user.Guid, userRequestUpdate);

       Assert.That(result, Is.Not.Null);
       Assert.That(result.Role, Is.EqualTo("User"));

       var updatedUser = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Guid == user.Guid);
       Assert.That(updatedUser, Is.Not.Null);
       Assert.That(updatedUser.IsDeleted, Is.EqualTo(userRequestUpdate.IsDeleted));
   }

   [Test]
   public async Task UpdateAsync_NotFound()
   {
       var nonExistentGuid = Guid.NewGuid().ToString();
       var userRequestUpdate = new UserRequestUpdate
       {
           
           Role = "User",
           IsDeleted = false
       };

       var result = await _userService.UpdateAsync(nonExistentGuid, userRequestUpdate);

       Assert.That(result, Is.Null);
   }

   [Test]
   public async Task DeleteByGuidAsync_UserNotFound_ThrowsUserNotFoundException()
   {
       var guid = "non-existing-guid";
       _dbContext.Usuarios.Add(new UserEntity { Guid = "existing-guid" , Username = "manolo@gmaail.com", Password = "12345678A"}); 
       await _dbContext.SaveChangesAsync();
       
       var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
           await _userService.DeleteByGuidAsync(guid)
       );
       ClassicAssert.AreEqual($"Usuario no encontrado con guid: {guid}", ex.Message);
   }
   [Test]
   public async Task DeleteByGuidAsync_UserExists_DeletesUserAndRelatedEntities()
   {
       // Arrange
       var guid = "existing-guid";
       var direccion = new Direccion { Calle = "calle de al lao", Letra = "A", Piso = "4", CodigoPostal = "28025", Numero = "1"};
       var userEntity = new UserEntity { Guid = guid, IsDeleted = false, Username = "manolo@gmail.com", Password = "12345678A"};
       _dbContext.Usuarios.Add(userEntity);
       _dbContext.Clientes.Add(new ClienteEntity { User = userEntity ,Telefono = "669843935", Guid = "existing-guid2", Nombre = "manolo",Email = "manolo@gmail.com" , Dni = "12345678Z", Apellidos = "guitierrez",Direccion = direccion});
       await _dbContext.SaveChangesAsync();

       var result = await _userService.DeleteByGuidAsync(guid);

   
       var user = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
       ClassicAssert.IsTrue(user.IsDeleted);
       ClassicAssert.AreEqual(result.Guid, guid);
    
       var cliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.User.Guid == guid);
       ClassicAssert.IsTrue(cliente.IsDeleted);
   }
    [Test]
    public async Task DeleteByGuidAsync_AccountWithBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var guid = "existing-guid";
        var producto = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
            { Guid = guid, Nombre = "producto", Descripcion = "producto", TipoProducto = "cuenta" };
        var userEntity = new UserEntity { Guid = guid, IsDeleted = false, Username = "manolo@gmail.com", Password = "12345678A"};
        var cuentaEntity = new CuentaEntity { Guid = guid, Saldo = 1000.0,ProductoId = producto.Id, Producto = producto.ToEntityFromModel() ,Iban = IbanGenerator.GenerateIban(),ClienteId = 1, IsDeleted = false };
        _dbContext.Usuarios.Add(userEntity);
        _dbContext.Cuentas.Add(cuentaEntity);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _userService.DeleteByGuidAsync(guid)
        );
        ClassicAssert.AreEqual("No se puede desactivar una cuenta con saldo", ex.Message);
    }

    [Test]
    public void GetAuthenticatedUser_Successfully()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        _dbContext.SaveChanges();
    
        var username = "username1";
        var userEntity = new UserEntity
        {
            Guid = "test-guid",
            Username = username,
            Password = BCrypt.Net.BCrypt.HashPassword("password1"),
            Role = Banco_VivesBank.User.Models.Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Usuarios.Add(userEntity);
        _dbContext.SaveChanges();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, username) }));
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
        var result = _userService.GetAuthenticatedUser();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(username));
    }
    [Test]
    public void GetAuthenticatedUser_NotAuthenticatedUser()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
        var result = _userService.GetAuthenticatedUser();
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public void GetAuthenticatedUser_NotFound()
    {
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        _dbContext.SaveChanges();
    
        var username = "username1";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, username) }));
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
        var result = _userService.GetAuthenticatedUser();
        Assert.That(result, Is.Null);
    }





}

