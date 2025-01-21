using Banco_VivesBank.Cliente.Controller;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Banco_VivesBank.Test.Cliente.Controller;

[TestFixture]
public class ClienteControllerTest
{
    private Mock<IClienteService> _clienteServiceMock;
    private ClienteController _clienteController;
    private Mock<PaginationLinksUtils> _paginationLinksUtils;

    [SetUp]
    public void SetUp()
    {
        _clienteServiceMock = new Mock<IClienteService>();
        _paginationLinksUtils = new Mock<PaginationLinksUtils>();
        _clienteController = new ClienteController(_clienteServiceMock.Object, _paginationLinksUtils.Object);
    }

    [Test]
    public async Task GetAll()
    {
        var response = new ClienteResponse
        {
            Guid = "guid",
            Nombre = "nombreTest",
            Apellidos =  "apellidosTest",
            Direccion = new Direccion {
                Calle = "calleTest",
                Numero = "numeroTest",
                CodigoPostal = "codigoPostalTest",
                Piso = "pisoTest",
                Letra = "letraTest"
            },
            Email = "emailTest",
            Telefono = "telefonoTest",
            FotoPerfil = "fotoPerfilTest",
            FotoDni = "fotoDniTest",
            UserResponse = new UserResponse 
            {
                Guid = "userGuid",
                Username = "usernameTest",
                Role = "roleTest",
                CreatedAt = "createdAtTest",
                UpdatedAt = "updatedAtTest",
                IsDeleted = false
            },
            CreatedAt = "createdAtTest",
            UpdatedAt = "updatedAtTest",
            IsDeleted = false
        };
        var clientes = new List<ClienteResponse> {response};
        _clienteServiceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(clientes);

        var result = await _clienteController.GetAll();
        
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.TypeOf<List<ClienteResponse>>());
        Assert.That(okResult.Value, Is.EqualTo(clientes));
    }

    [Test]
    public async Task GetAll_EmptyList()
    {
        var clientes = new List<ClienteResponse> {};
        _clienteServiceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(clientes);

        var result = await _clienteController.GetAll();
        
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(clientes));
    }
    
    [Test]
    public async Task GetByGuid()
    {
        var response = new ClienteResponse
        {
            Guid = "guid",
            Nombre = "nombreTest",
            Apellidos =  "apellidosTest",
            Direccion = new Direccion {
                Calle = "calleTest",
                Numero = "numeroTest",
                CodigoPostal = "codigoPostalTest",
                Piso = "pisoTest",
                Letra = "letraTest"
            },
            Email = "emailTest",
            Telefono = "telefonoTest",
            FotoPerfil = "fotoPerfilTest",
            FotoDni = "fotoDniTest",
            UserResponse = new UserResponse
            {
                Guid = "userGuid",
                Username = "usernameTest",
                Role = "roleTest",
                CreatedAt = "createdAtTest",
                UpdatedAt = "updatedAtTest",
                IsDeleted = false
            },
            CreatedAt = "createdAtTest",
            UpdatedAt = "updatedAtTest",
            IsDeleted = false
        };
        
        _clienteServiceMock.Setup(service => service.GetByGuidAsync(It.IsAny<string>()))
            .ReturnsAsync(response);
        var result = await _clienteController.GetByGuid("guid");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());


        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<ClienteResponse>());


        var clienteResult = okResult.Value as ClienteResponse;
        Assert.That(clienteResult, Is.EqualTo(response));
    }
    
    [Test]
    public async Task GetByGuid_ReturnsNotFound_WhenClienteDoesNotExist()
    {
        _clienteServiceMock.Setup(service => service.GetByGuidAsync(It.IsAny<string>())).ReturnsAsync((ClienteResponse)null);

        var result = await _clienteController.GetByGuid("nonexistent-guid");

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult?.Value, Is.EqualTo("No se ha encontrado cliente con guid: nonexistent-guid"));
    }

    [Test]
    public async Task Create()
    {
        // Arrange
        var clienteRequest = new ClienteRequest
        {
            Dni = "asdas",
            Nombre = "nombreTest",
            Apellidos = "apellidosTest",
            Calle = "calleTest",
            Numero = "numeroTest",
            CodigoPostal = "codigoPostalTest",
            Piso = "pisoTest",
            Letra = "letraTest",
            Email = "emailTest",
            Telefono = "telefonoTest",
            UserGuid = "userIdTest"
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "guid",
            Nombre = "nombreTest",
            Apellidos = "apellidosTest",
            Direccion = new Direccion
            {
                Calle = "calleTest",
                Numero = "numeroTest",
                CodigoPostal = "codigoPostalTest",
                Piso = "pisoTest",
                Letra = "letraTest"
            },
            Email = clienteRequest.Email,
            Telefono = clienteRequest.Telefono,
            UserResponse = new UserResponse
            {
                Guid = "userGuid",
                Username = "usernameTest",
                Role = "roleTest",
                CreatedAt = "createdAtTest",
                UpdatedAt = "updatedAtTest",
                IsDeleted = false
            },
            CreatedAt = "createdAtTest",
            UpdatedAt = "updatedAtTest",
            IsDeleted = false
        };

        _clienteServiceMock.Setup(service => service.CreateAsync(It.IsAny<ClienteRequest>()))
            .ReturnsAsync(clienteResponse);

        var result = await _clienteController.Create(clienteRequest);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(clienteResponse));
    }

    [Test]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        _clienteController.ModelState.AddModelError("Nombre", "El campo es obligatorio");
        var clienteRequest = new ClienteRequest();

        var result = await _clienteController.Create(clienteRequest);
        
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_ThrowsClienteException_ReturnsBadRequest()
    {
        var clienteRequest = new ClienteRequest
        {
            Nombre = "nombreTest",
            Apellidos = "apellidosTest"
        };

        _clienteServiceMock.Setup(service => service.CreateAsync(It.IsAny<ClienteRequest>()))
            .ThrowsAsync(new ClienteException("Cliente ya existe"));

        var result = await _clienteController.Create(clienteRequest);
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Cliente ya existe"));
    }

    [Test]
    public async Task Create_ThrowsUserException_ReturnsNotFound()
    {
        var clienteRequest = new ClienteRequest
        {
            Nombre = "nombreTest",
            Apellidos = "apellidosTest"
        };

        _clienteServiceMock.Setup(service => service.CreateAsync(It.IsAny<ClienteRequest>()))
            .ThrowsAsync(new UserException("Usuario no encontrado"));

        var result = await _clienteController.Create(clienteRequest);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo("Usuario no encontrado"));
    }

    [Test]
    public async Task UpdateCliente_ValidRequest_ReturnsOk()
    {
        var guid = "valid-guid";
        var clienteRequestUpdate = new ClienteRequestUpdate
        {
            Nombre = "NuevoNombre",
            Apellidos = "NuevoApellido",
            Calle = "NuevaCalle",
            Numero = "NuevoNumero",
            CodigoPostal = "NuevoCodigoPostal",
            Piso = "NuevoPiso",
            Letra = "NuevaLetra",
            Email = "nuevoEmail@test.com",
            Telefono = "123456789"
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = guid,
            Nombre = clienteRequestUpdate.Nombre,
            Apellidos = clienteRequestUpdate.Apellidos,
            Direccion =new Direccion
            {
                Calle = "NuevaCalle",
                Numero = "NuevoNumero",
                CodigoPostal = "NuevoCodigoPostal",
                Piso = "NuevoPiso",
                Letra = "NuevaLetra",
            },
            Email = clienteRequestUpdate.Email,
            Telefono = clienteRequestUpdate.Telefono,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-10",
            IsDeleted = false
        };

        _clienteServiceMock.Setup(service => service.UpdateAsync(guid, clienteRequestUpdate))
            .ReturnsAsync(clienteResponse);

        var result = await _clienteController.UpdateCliente(guid, clienteRequestUpdate);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(clienteResponse));
    }

    [Test]
    public async Task UpdateCliente_InvalidModelState_ReturnsBadRequest()
    {
        var guid = "valid-guid";
        _clienteController.ModelState.AddModelError("Nombre", "El campo es requerido");
        var clienteRequestUpdate = new ClienteRequestUpdate();

        var result = await _clienteController.UpdateCliente(guid, clienteRequestUpdate);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateCliente_NotFound_ReturnsNotFound()
    {
        var guid = "nonexistent-guid";
        var clienteRequestUpdate = new ClienteRequestUpdate
        {
            Nombre = "NuevoNombre",
            Apellidos = "NuevoApellido"
        };

        _clienteServiceMock.Setup(service => service.UpdateAsync(guid, clienteRequestUpdate))
            .ReturnsAsync((ClienteResponse)null);

        var result = await _clienteController.UpdateCliente(guid, clienteRequestUpdate);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha podido actualizar el cliente con guid: {guid}"));
    }

    [Test]
    public async Task UpdateCliente_ThrowsClienteException_ReturnsBadRequest()
    {
        var guid = "valid-guid";
        var clienteRequestUpdate = new ClienteRequestUpdate
        {
            Nombre = "NuevoNombre",
            Apellidos = "NuevoApellido"
        };

        _clienteServiceMock.Setup(service => service.UpdateAsync(guid, clienteRequestUpdate))
            .ThrowsAsync(new ClienteException("Error al actualizar el cliente"));

        var result = await _clienteController.UpdateCliente(guid, clienteRequestUpdate);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Error al actualizar el cliente"));
    }
    
    [Test]
    public async Task DeleteByGuid_ValidGuid_ReturnsOk()
    {
        var guid = "valid-guid";
        var clienteResponse = new ClienteResponse
        {
            Guid = guid,
            Nombre = "NombreTest",
            Apellidos = "ApellidosTest",
            IsDeleted = true
        };

        _clienteServiceMock.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync(clienteResponse);

        var result = await _clienteController.DeleteByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(clienteResponse));
    }

    [Test]
    public async Task DeleteByGuid_NotFound_ReturnsNotFound()
    {
        var guid = "nonexistent-guid";

        _clienteServiceMock.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync((ClienteResponse)null);

        var result = await _clienteController.DeleteByGuid(guid);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha podido borrar el usuario con guid: {guid}"));
    }
}