using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Controller;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;

namespace Test.Movimientos.Controller;
[TestFixture]
public class DomiciliacionControllerTests
{
    private Mock<IDomiciliacionService> _mockDomiciliacionService;
    private Mock<IUserService> _userServiceMock;
    private DomiciliacionController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockDomiciliacionService = new Mock<IDomiciliacionService>();
        _userServiceMock = new Mock<IUserService>();
        _controller = new DomiciliacionController(_mockDomiciliacionService.Object, _userServiceMock.Object);
    }

    [Test]
    public async Task GetAll()
    {
        var mockResponseList = new List<DomiciliacionResponse>
        {
            new DomiciliacionResponse
            {
                Guid = "1",
                ClienteGuid = "cliente1",
                Importe = 100.0,
                Periodicidad = "Mensual",
                Activa = true,
                FechaInicio = "01/01/2025",
                UltimaEjecuccion = "01/02/2025"
            },
            new DomiciliacionResponse
            {
                Guid = "2",
                ClienteGuid = "cliente2",
                Importe = 200.0,
                Periodicidad = "Anual",
                Activa = false,
                FechaInicio = "02/01/2025",
                UltimaEjecuccion = "02/02/2025"
            }
        };

        _mockDomiciliacionService.Setup(service => service.GetAllAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAllDomiciliaciones();
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockResponseList.Count));

        for (int i = 0; i < responseList.Count; i++)
        {
            Assert.That(responseList[i].Guid, Is.EqualTo(mockResponseList[i].Guid));
            Assert.That(responseList[i].ClienteGuid, Is.EqualTo(mockResponseList[i].ClienteGuid));
            Assert.That(responseList[i].Importe, Is.EqualTo(mockResponseList[i].Importe));
            Assert.That(responseList[i].Periodicidad, Is.EqualTo(mockResponseList[i].Periodicidad));
            Assert.That(responseList[i].Activa, Is.EqualTo(mockResponseList[i].Activa));
            Assert.That(responseList[i].FechaInicio, Is.EqualTo(mockResponseList[i].FechaInicio));
            Assert.That(responseList[i].UltimaEjecuccion, Is.EqualTo(mockResponseList[i].UltimaEjecuccion));
        }
    }

    [Test]
    public async Task GetAll_ListaVacia()
    {
        
        var mockResponseList = new List<DomiciliacionResponse>();

        _mockDomiciliacionService.Setup(service => service.GetAllAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAllDomiciliaciones();
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(0));
    }
    
    
    [Test]
    public async Task GetByGuid()
    {
        var domiciliacionGuid = "1";
        var mockResponse = new DomiciliacionResponse
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente1",
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true,
            FechaInicio = "01/01/2025",
            UltimaEjecuccion = "01/02/2025"
        };

        _mockDomiciliacionService.Setup(service => service.GetByGuidAsync(domiciliacionGuid))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.GetDomiciliacionByGuid(domiciliacionGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var response = okResult.Value as DomiciliacionResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Guid, Is.EqualTo(mockResponse.Guid));
        Assert.That(response.ClienteGuid, Is.EqualTo(mockResponse.ClienteGuid));
        Assert.That(response.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(response.Periodicidad, Is.EqualTo(mockResponse.Periodicidad));
        Assert.That(response.Activa, Is.EqualTo(mockResponse.Activa));
        Assert.That(response.FechaInicio, Is.EqualTo(mockResponse.FechaInicio));
        Assert.That(response.UltimaEjecuccion, Is.EqualTo(mockResponse.UltimaEjecuccion));
    }

    
    [Test]
    public async Task GetDomiciliacionByGuid_NotFound()
    {
        var domiciliacionGuid = "1"; 
        var errorMessage = $"No se ha encontrado la domiciliación con guid: {domiciliacionGuid}";

        _mockDomiciliacionService.Setup(service => service.GetByGuidAsync(domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null);

        var result = await _controller.GetDomiciliacionByGuid(domiciliacionGuid);

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null, "Expected NotFoundObjectResult.");

        Assert.That(notFoundResult.Value, Is.Not.Null, "Expected a valid response object.");

        var responseValue = Newtonsoft.Json.JsonConvert.SerializeObject(notFoundResult.Value);
        var jObject = Newtonsoft.Json.Linq.JObject.Parse(responseValue);

        var message = jObject["message"]?.ToString();
        Assert.That(message, Is.EqualTo(errorMessage), "Expected error message not found.");
    }
    
    [Test]
    public async Task GetByGuid_MovimientoException()
    {
        var domiciliacionGuid = "1";
        var exceptionMessage = "Error al procesar la solicitud";

        _mockDomiciliacionService.Setup(service => service.GetByGuidAsync(domiciliacionGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.GetDomiciliacionByGuid(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);

        var response = badRequestResult.Value;
        var messageProperty = response?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(response).ToString();
            Assert.That(actualMessage, Is.EqualTo(exceptionMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
     [Test]
    public async Task GetByClienteGuid()
    {
        var clienteGuid = "cliente1";
        var mockResponseList = new List<DomiciliacionResponse>
        {
            new DomiciliacionResponse
            {
                Guid = "1",
                ClienteGuid = clienteGuid,
                Importe = 100.0,
                Periodicidad = "Mensual",
                Activa = true,
                FechaInicio = "01/01/2025",
                UltimaEjecuccion = "01/02/2025"
            },
            new DomiciliacionResponse
            {
                Guid = "2",
                ClienteGuid = clienteGuid,
                Importe = 200.0,
                Periodicidad = "Mensual",
                Activa = false,
                FechaInicio = "02/01/2025",
                UltimaEjecuccion = "02/02/2025"
            }
        };

        _mockDomiciliacionService.Setup(service => service.GetByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetDomiciliacionesByClienteGuid(clienteGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockResponseList.Count));

        foreach (var response in responseList)
        {
            Assert.That(response.ClienteGuid, Is.EqualTo(clienteGuid));
        }
    }

    [Test]
    public async Task GetByClienteGuid_NotFound()
    {
        var clienteGuid = "cliente1";

        _mockDomiciliacionService.Setup(service => service.GetByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(new List<DomiciliacionResponse>());
        
        var result = await _controller.GetDomiciliacionesByClienteGuid(clienteGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(0));
    }
    
     [Test]
    public async Task GetMyDomiciliaciones_Success()
    {
        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        
        var mockDomiciliaciones = new List<DomiciliacionResponse>
        {
            new DomiciliacionResponse
            {
                Guid = "1",
                ClienteGuid = authenticatedUser.Guid,
                Acreedor = "Empresa1",
                IbanEmpresa = "ES1234567890",
                IbanCliente = "ES0987654321",
                Importe = 100.0,
                Periodicidad = "Mensual",
                Activa = true,
                FechaInicio = "01/01/2025",
                UltimaEjecuccion = "01/02/2025"
            },
            new DomiciliacionResponse
            {
                Guid = "2",
                ClienteGuid = authenticatedUser.Guid,
                Acreedor = "Empresa2",
                IbanEmpresa = "ES1111222233",
                IbanCliente = "ES3333222211",
                Importe = 200.0,
                Periodicidad = "Mensual",
                Activa = false,
                FechaInicio = "02/01/2025",
                UltimaEjecuccion = "02/02/2025"
            }
        };
        
        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);
        
        _mockDomiciliacionService.Setup(service => service.GetMyDomiciliaciones(authenticatedUser))
            .ReturnsAsync(mockDomiciliaciones);
        
        var result = await _controller.GetMyDomiciliaciones();
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockDomiciliaciones.Count));

       
        for (int i = 0; i < mockDomiciliaciones.Count; i++)
        {
            Assert.That(responseList[i].Guid, Is.EqualTo(mockDomiciliaciones[i].Guid));
            Assert.That(responseList[i].ClienteGuid, Is.EqualTo(mockDomiciliaciones[i].ClienteGuid));
            Assert.That(responseList[i].Acreedor, Is.EqualTo(mockDomiciliaciones[i].Acreedor));
            Assert.That(responseList[i].IbanEmpresa, Is.EqualTo(mockDomiciliaciones[i].IbanEmpresa));
            Assert.That(responseList[i].IbanCliente, Is.EqualTo(mockDomiciliaciones[i].IbanCliente));
            Assert.That(responseList[i].Importe, Is.EqualTo(mockDomiciliaciones[i].Importe));
            Assert.That(responseList[i].Periodicidad, Is.EqualTo(mockDomiciliaciones[i].Periodicidad));
            Assert.That(responseList[i].Activa, Is.EqualTo(mockDomiciliaciones[i].Activa));
            Assert.That(responseList[i].FechaInicio, Is.EqualTo(mockDomiciliaciones[i].FechaInicio));
            Assert.That(responseList[i].UltimaEjecuccion, Is.EqualTo(mockDomiciliaciones[i].UltimaEjecuccion));
        }
    }

  

    [Test]
    public async Task GetMyDomiciliaciones_ListaVacia()
    {
        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.GetMyDomiciliaciones(authenticatedUser))
            .ReturnsAsync(new List<DomiciliacionResponse>());

        var result = await _controller.GetMyDomiciliaciones();
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<DomiciliacionResponse>;
        
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(0));
    }
    
    
    [Test]
    public async Task CreateDomiciliacion()
    {
        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Empresa1",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual"
        };

        var expectedResponse = new DomiciliacionResponse
        {
            Guid = "1",
            ClienteGuid = authenticatedUser.Guid,
            Acreedor = "Empresa1",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true,
            FechaInicio = "01/01/2025",
            UltimaEjecuccion = "01/02/2025"
        };

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.CreateAsync(authenticatedUser, domiciliacionRequest))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var response = okResult.Value as DomiciliacionResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Guid, Is.EqualTo(expectedResponse.Guid));
        Assert.That(response.ClienteGuid, Is.EqualTo(expectedResponse.ClienteGuid));
    }

    [Test]
    public async Task CreateDomiciliacion_UserNotFound()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Empresa1",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual"
        };

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns((Banco_VivesBank.User.Models.User)null);

        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;
    
        Assert.That(notFoundResult, Is.Not.Null);

        var responseValue = notFoundResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo("No se ha podido identificar al usuario logeado"));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }

    [Test]
    public async Task CreateDomiciliacion_Invalido()
    {
        _controller.ModelState.AddModelError("Acreedor", "El acreedor es un campo obligatorio");

        var domiciliacionRequest = new DomiciliacionRequest
        {
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual"
        };

        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task CreateDomiciliacion_ClienteException()
    {
        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Empresa1",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual"
        };

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.CreateAsync(authenticatedUser, domiciliacionRequest))
            .ThrowsAsync(new ClienteException("Cliente no encontrado"));

        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;
    
        Assert.That(notFoundResult, Is.Not.Null);

        var responseValue = notFoundResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo("Cliente no encontrado"));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }


    [Test]
    public async Task CreateDomiciliacion_SaldoCuentaInsuficiente()
    {
        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Empresa1",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 100.0,
            Periodicidad = "Mensual"
        };

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.CreateAsync(authenticatedUser, domiciliacionRequest))
            .ThrowsAsync(new SaldoCuentaInsuficientException("Saldo insuficiente"));

        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
    
        Assert.That(badRequestResult, Is.Not.Null);

        var responseValue = badRequestResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo("Saldo insuficiente"));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
    
    [Test]
    public async Task DesactivateDomiciliacion()
    {
        var domiciliacionGuid = "domiciliacion1";
        var expectedResponse = new DomiciliacionResponse
        {
            Guid = "domiciliacion1",
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = false,
            FechaInicio = "2025-01-01",
            UltimaEjecuccion = "2025-01-15"
        };

        _mockDomiciliacionService.Setup(service => service.DesactivateDomiciliacionAsync(domiciliacionGuid))
            .ReturnsAsync(expectedResponse);
        
        var result = await _controller.DesactivateDomiciliacion(domiciliacionGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var response = okResult.Value as DomiciliacionResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Guid, Is.EqualTo(expectedResponse.Guid));
        Assert.That(response.ClienteGuid, Is.EqualTo(expectedResponse.ClienteGuid));
        Assert.That(response.Acreedor, Is.EqualTo(expectedResponse.Acreedor));
        Assert.That(response.IbanEmpresa, Is.EqualTo(expectedResponse.IbanEmpresa));
        Assert.That(response.IbanCliente, Is.EqualTo(expectedResponse.IbanCliente));
        Assert.That(response.Importe, Is.EqualTo(expectedResponse.Importe));
        Assert.That(response.Periodicidad, Is.EqualTo(expectedResponse.Periodicidad));
        Assert.That(response.Activa, Is.False); 
        Assert.That(response.FechaInicio, Is.EqualTo(expectedResponse.FechaInicio));
        Assert.That(response.UltimaEjecuccion, Is.EqualTo(expectedResponse.UltimaEjecuccion));
    }
    
    [Test]
    public async Task DesactivateDomiciliacion_NotFound()
    {
        var domiciliacionGuid = "domiciliacion1";

        _mockDomiciliacionService.Setup(service => service.DesactivateDomiciliacionAsync(domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null);

        var result = await _controller.DesactivateDomiciliacion(domiciliacionGuid);

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);  
        
        var responseValue = JObject.FromObject(notFoundResult.Value);
        Assert.That(responseValue["message"].ToString(), Is.EqualTo($"No se ha encontrado domiciliacion con guid {domiciliacionGuid}"));
    }
    
    [Test]
    public async Task DesactivateDomiciliacion_MovimientoException()
    {
        var domiciliacionGuid = "domiciliacion1";
        var exceptionMessage = "Error al desactivar la domiciliación";

        _mockDomiciliacionService.Setup(service => service.DesactivateDomiciliacionAsync(domiciliacionGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.DesactivateDomiciliacion(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);

        var responseValue = badRequestResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");
        
        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue).ToString();
            Assert.That(actualMessage, Is.EqualTo(exceptionMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacion_UserNotFound()
    {
        var domiciliacionGuid = "domiciliacion1";
        var errorMessage = "No se ha podido identificar al usuario logeado";

        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns((Banco_VivesBank.User.Models.User)null);

        var result = await _controller.DesactivateMyDomiciliacion(domiciliacionGuid);
        var notFoundResult = result.Result as NotFoundObjectResult;
    
        Assert.That(notFoundResult, Is.Not.Null);

        var responseValue = notFoundResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo(errorMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
    
    [Test]
    public async Task DesactivateMyDomiciliacion_DomiciliacionNotFound()
    {
        var domiciliacionGuid = "domiciliacion1";
        var errorMessage = $"No se ha encontrado domiciliacion con guid {domiciliacionGuid}";

        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.DesactivateMyDomiciliacionAsync(authenticatedUser, domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null);

        var result = await _controller.DesactivateMyDomiciliacion(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;
    
        Assert.That(badRequestResult, Is.Not.Null);

        var responseValue = badRequestResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo(errorMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacion_MovimientoNoPertenecienteAlUsuarioAutenticadoException()
    {
        var domiciliacionGuid = "domiciliacion1";
        var exceptionMessage = "Movimiento no perteneciente al usuario autenticado";

        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.DesactivateMyDomiciliacionAsync(authenticatedUser, domiciliacionGuid))
            .ThrowsAsync(new MovimientoNoPertenecienteAlUsuarioAutenticadoException(exceptionMessage));

        var result = await _controller.DesactivateMyDomiciliacion(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);

        var responseValue = badRequestResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo(exceptionMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacion_MovimientoException()
    {
        var domiciliacionGuid = "domiciliacion1";
        var exceptionMessage = "Error al desactivar la domiciliación";

        var authenticatedUser = new Banco_VivesBank.User.Models.User { Guid = "cliente1", Username = "usuario123" };
        _userServiceMock.Setup(service => service.GetAuthenticatedUser())
            .Returns(authenticatedUser);

        _mockDomiciliacionService.Setup(service => service.DesactivateMyDomiciliacionAsync(authenticatedUser, domiciliacionGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.DesactivateMyDomiciliacion(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);

        var responseValue = badRequestResult.Value;
        var messageProperty = responseValue?.GetType().GetProperty("message");

        if (messageProperty != null)
        {
            var actualMessage = messageProperty.GetValue(responseValue)?.ToString();
            Assert.That(actualMessage, Is.EqualTo(exceptionMessage));
        }
        else
        {
            Assert.Fail("La propiedad 'message' no fue encontrada en la respuesta.");
        }
    }
}