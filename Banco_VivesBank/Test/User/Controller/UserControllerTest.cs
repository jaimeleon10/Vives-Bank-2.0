using Banco_VivesBank.User.Controller;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.User.Controller;
[TestFixture]
public class UserControllerTest
{
    private Mock<IUserService> _userServiceMock;
    private UserController _userController;
    
    
    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _userController = new UserController(_userServiceMock.Object);
    }
    
    [Test]
    public async Task GetAll()
    {
        var userResponse = new UserResponse
        {
            Guid = "guid",
            Username = "username",
            Role = "USER",
        };
        
        _userServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<UserResponse> { userResponse });
        
        var result = await _userController.GetAll();
    }
    
    
    [Test]
    public async Task GetAll_EmptyList()
    {
        _userServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<UserResponse>());
        
        var result = await _userController.GetAll();
    }
    
    [Test]
    public async Task GetByGuid()
    {
        var userResponse = new UserResponse
        {
            Guid = "guid",
            Username = "username",
            Role = "USER",
        };
        
        _userServiceMock.Setup(x => x.GetByGuidAsync(It.IsAny<string>())).ReturnsAsync(userResponse);
        
        var result = await _userController.GetByGuid("guid");
    }
    
    
    [Test]
    public async Task GetByGuid_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _userServiceMock.Setup(service => service.GetByGuidAsync(It.IsAny<string>())).ReturnsAsync((UserResponse)null);

        var result = await _userController.GetByGuid("nonexistent-guid");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult?.Value, Is.EqualTo("No se ha encontrado usuario con guid: nonexistent-guid"));
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
        
        var userResponse = new UserResponse
        {
            Guid = "guid",
            Username = "username",
            Role = "USER",
        };
        
        _userServiceMock.Setup(x => x.CreateAsync(It.IsAny<UserRequest>())).ReturnsAsync(userResponse);
        
        var result = await _userController.Create(userRequest);
    }
    
    [Test]
    public async Task Update()
    {
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };
        
        var userResponse = new UserResponse
        {
            Guid = "guid",
            Username = "username",
            Role = "USER",
        };
        
        _userServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UserRequest>())).ReturnsAsync(userResponse);
        
        var result = await _userController.Update("guid", userRequest);
    }
    
    [Test]
    public async Task UpdateUser_InvalidModelState_ReturnsBadRequest()
    {
        var guid = "valid-guid";
        _userController.ModelState.AddModelError("UserName", "El campo es requerido");
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        var result = await _userController.Update(guid, userRequest);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
   /*[Test]
    public async Task UpdateUser_ThrowsUserException_ReturnsBadRequest()
    {
        var guid = "valid-guid";
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        _userServiceMock.Setup(service => service.UpdateAsync(guid, userRequest))
            .ThrowsAsync(new UserException("UserException message"));

        var result = await _userController.Update(guid, userRequest);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("UserException message"));
    }
    */
    [Test]
    public async Task DeleteByGuid_ValidGuid_ReturnsOk()
    {
        var guid = "valid-guid";
        var userResponse = new UserResponse
        {
            Guid = guid,
            Username = "username",
            Role = "USER",
            IsDeleted = true
        };

        _userServiceMock.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync(userResponse);

        var result = await _userController.DeleteByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(userResponse));
    }
    
    [Test]
    public async Task DeleteByGuid_NotFound_ReturnsNotFound()
    {
        var guid = "nonexistent-guid";

        _userServiceMock.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync((UserResponse)null);

        var result = await _userController.DeleteByGuid(guid);
        
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha podido borrar el usuario con guid: {guid}"));
    }
    
    
    [Test]
    
    public async Task GetByUsername()
    {
        var userResponse = new UserResponse
        {
            Guid = "guid",
            Username = "username",
            Role = "USER",
        };
        
        _userServiceMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(userResponse);
        
        var result = await _userController.GetByUsername("username");
    }
    
    
    [Test]
    public async Task GetByUsername_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _userServiceMock.Setup(service => service.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((UserResponse)null);

        var result = await _userController.GetByUsername("nonexistent-username");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult?.Value, Is.EqualTo("No se ha encontrado usuario con nombre de usuario: nonexistent-username"));
    }
    
    [Test]
    public async Task Create_InvalidModelState_ReturnsBadRequest()
    {
        _userController.ModelState.AddModelError("UserName", "El campo es requerido");
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        var result = await _userController.Create(userRequest);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Create_ThrowsUserException_ReturnsBadRequest()
    {
        var userRequest = new UserRequest
        {
            Username = "username",
            Password = "password",
            Role = "USER",
        };

        _userServiceMock.Setup(service => service.CreateAsync(userRequest))
            .ThrowsAsync(new UserException("UserException message"));

        var result = await _userController.Create(userRequest);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("UserException message"));
    }
}