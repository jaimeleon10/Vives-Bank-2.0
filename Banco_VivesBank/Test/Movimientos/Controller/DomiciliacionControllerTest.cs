using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Controller;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Movimientos.Controller;
[TestFixture]
public class DomiciliacionControllerTests
{
    private Mock<IDomiciliacionService> _mockDomiciliacionService;
    private DomiciliacionController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockDomiciliacionService = new Mock<IDomiciliacionService>();
        _controller = new DomiciliacionController(_mockDomiciliacionService.Object);
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
    public async Task GetByGuid_NotExist()
    {
      
        var domiciliacionGuid = "1";
        var errorMessage = $"No se ha encontrado la domiciliación con guid: {domiciliacionGuid}";

        _mockDomiciliacionService.Setup(service => service.GetByGuidAsync(domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null);
        
        var result = await _controller.GetDomiciliacionByGuid(domiciliacionGuid);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(errorMessage));
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
        Assert.That(badRequestResult.Value, Is.EqualTo(exceptionMessage));
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
    public async Task Create()
    {
        var request = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098",  
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        var expectedResponse = new DomiciliacionResponse
        {
            Guid = "domiciliacion1",
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        _mockDomiciliacionService.Setup(service => service.CreateAsync(request))
            .ReturnsAsync(expectedResponse);
        
        var result = await _controller.CreateDomiciliacion(request);
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
        Assert.That(response.Activa, Is.EqualTo(expectedResponse.Activa));
    }

    [Test]
    public async Task Create_ClienteException()
    {
        var request = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098",  
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        var exceptionMessage = "El cliente no existe";

        _mockDomiciliacionService.Setup(service => service.CreateAsync(request))
            .ThrowsAsync(new ClienteException(exceptionMessage));
        
        var result = await _controller.CreateDomiciliacion(request);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }

   

    [Test]
    public async Task Create_SaldoCuentaInsuficientException()
    {
        var request = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098", 
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        var exceptionMessage = "Saldo insuficiente";

        _mockDomiciliacionService.Setup(service => service.CreateAsync(request))
            .ThrowsAsync(new SaldoCuentaInsuficientException(exceptionMessage));
        
        var result = await _controller.CreateDomiciliacion(request);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Create_CuentaException()
    {
     
        var request = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098", 
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        var exceptionMessage = "La cuenta no existe";

        _mockDomiciliacionService.Setup(service => service.CreateAsync(request))
            .ThrowsAsync(new CuentaException(exceptionMessage));

      
        var result = await _controller.CreateDomiciliacion(request);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Create_MovimientoException()
    {
        var request = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098", 
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };

        var exceptionMessage = "Error al crear la domiciliación";

        _mockDomiciliacionService.Setup(service => service.CreateAsync(request))
            .ThrowsAsync(new MovimientoException(exceptionMessage));
        
        var result = await _controller.CreateDomiciliacion(request);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    [Test]
    public async Task Create_Invalido()
    {
        _controller.ModelState.AddModelError("ClienteGuid", "El guid del cliente es un campo obligatorio");

        var request = new DomiciliacionRequest
        {
            Acreedor = "AcreedorX",
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098",  
            Importe = 100.0,
            Periodicidad = "Mensual",
            Activa = true
        };
        
        var result = await _controller.CreateDomiciliacion(request);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.Not.Null);
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
        var errorMessage = $"No se ha encontrado domiciliacion con guid {domiciliacionGuid}";

        _mockDomiciliacionService.Setup(service => service.DesactivateDomiciliacionAsync(domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null);
        
        var result = await _controller.DesactivateDomiciliacion(domiciliacionGuid);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(errorMessage));
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
        Assert.That(badRequestResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    [Test]
    public async Task DesactivateDomiciliacion_ShouldReturnNotFound_WhenDomiciliacionDoesNotExist()
    {
       
        var domiciliacionGuid = "nonexistentGuid";
        var errorMessage = "No se ha encontrado domiciliacion con guid nonexistentGuid";

        _mockDomiciliacionService.Setup(service => service.DesactivateDomiciliacionAsync(domiciliacionGuid))
            .ReturnsAsync((DomiciliacionResponse)null); 
        
        var result = await _controller.DesactivateDomiciliacion(domiciliacionGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(errorMessage));
    }
}
