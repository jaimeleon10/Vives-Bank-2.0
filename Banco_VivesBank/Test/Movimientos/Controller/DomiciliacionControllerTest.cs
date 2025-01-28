using Banco_VivesBank.Movimientos.Controller;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Microsoft.AspNetCore.Mvc;
using Moq;
/*
namespace Test.Movimientos.Controller;
[TestFixture]
public class DomiciliacionControllerTest
{
    
    private Mock<IDomiciliacionService> _mockDomiciliacionService;
    private MovimientoController _controller;

       [Test]
    public async Task GetAllDomiciliaciones()
    {
        var mockResponseList = new List<DomiciliacionResponse>
        {
            new DomiciliacionResponse { Guid = "1", ClienteGuid = "cliente1", Importe = 100.0, Periodicidad = "Mensual", Activa = true, FechaInicio = "01/01/2025", UltimaEjecuccion = "01/02/2025" },
            new DomiciliacionResponse { Guid = "2", ClienteGuid = "cliente2", Importe = 200.0, Periodicidad = "Mensual", Activa = false, FechaInicio = "02/01/2025", UltimaEjecuccion = "02/02/2025" }
        };

        _mockDomiciliacionService.Setup(service => service.GetAllDomiciliacionesAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAllDomiciliaciones();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    
        var responseList = okResult.Value as List<DomiciliacionResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockResponseList.Count));
        
        foreach (var response in responseList)
        {
            Assert.That(response.Guid, Is.Not.Null);
            Assert.That(response.ClienteGuid, Is.Not.Null);
            Assert.That(response.Importe, Is.GreaterThan(0));
            Assert.That(response.Periodicidad, Is.Not.Null);
            Assert.That(response.FechaInicio, Is.Not.Null);
            Assert.That(response.UltimaEjecuccion, Is.Not.Null);
            Assert.That(response.Activa, Is.TypeOf<bool>());
        }
    }
    
    [Test]
    public async Task GetAllDomiciliaciones_DomiciliacionesNotFound()
    {
       
        var mockResponseList = new List<DomiciliacionResponse>(); 

        _mockDomiciliacionService.Setup(service => service.GetAllDomiciliacionesAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAllDomiciliaciones();
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo("No se ha encontrado ninguna domiciliación"));
    }
    
    [Test]
    public async Task GetDomiciliacionesByClienteGuid()
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

    _mockDomiciliacionService.Setup(service => service.GetDomiciliacionesByClienteGuidAsync(clienteGuid))
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
        
        foreach (var response in responseList)
        {
            Assert.That(response.Guid, Is.Not.Null);
            Assert.That(response.ClienteGuid, Is.Not.Null);
            Assert.That(response.Importe, Is.GreaterThan(0));
            Assert.That(response.Periodicidad, Is.Not.Null);
            Assert.That(response.FechaInicio, Is.Not.Null);
            Assert.That(response.UltimaEjecuccion, Is.Not.Null);
            Assert.That(response.Activa, Is.TypeOf<bool>());
        }
    }
    
    [Test]
    public async Task GetDomiciliacionesByClienteGuid_DomiciliacionesNotFound()
    {
        var clienteGuid = "cliente1";
        var mockResponseList = new List<DomiciliacionResponse>(); 

        _mockDomiciliacionService.Setup(service => service.GetDomiciliacionesByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetDomiciliacionesByClienteGuid(clienteGuid);
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha encontrado ninguna domiciliación para el cliente con guid: {clienteGuid}"));
    }
    
    [Test]
    public async Task CreateDomiciliacion()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente1",  
            Acreedor = "AcreedorTest", 
            IbanEmpresa = "ES1234567890123456789012", 
            IbanCliente = "ES9876543210987654321098", 
            Importe = 1000.0,  
            Periodicidad = "Mensual",  
            Activa = true  
        };
        
        var mockResponse = new DomiciliacionResponse
        {
            Guid = "1", 
            ClienteGuid = "cliente1", 
            Acreedor = "AcreedorTest", 
            IbanEmpresa = "ES1234567890123456789012", 
            IbanCliente = "ES9876543210987654321098", 
            Importe = 1000.0,  
            Periodicidad = "Mensual", 
            Activa = true,  
            FechaInicio = "01/01/2025", 
            UltimaEjecuccion = "01/02/2025" 
        };
        
        _mockDomiciliacionService.Setup(service => service.CreateDomiciliacionAsync(domiciliacionRequest))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.CreateDomiciliacion(domiciliacionRequest);
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var responseValue = okResult.Value as DomiciliacionResponse;
        Assert.That(responseValue, Is.Not.Null);
        
        Assert.That(responseValue.Guid, Is.EqualTo(mockResponse.Guid));
        Assert.That(responseValue.ClienteGuid, Is.EqualTo(mockResponse.ClienteGuid));
        Assert.That(responseValue.Acreedor, Is.EqualTo(mockResponse.Acreedor));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.Periodicidad, Is.EqualTo(mockResponse.Periodicidad));
        Assert.That(responseValue.Activa, Is.EqualTo(mockResponse.Activa));
        Assert.That(responseValue.FechaInicio, Is.EqualTo(mockResponse.FechaInicio));
        Assert.That(responseValue.UltimaEjecuccion, Is.EqualTo(mockResponse.UltimaEjecuccion));
        
        Assert.That(responseValue.IbanEmpresa, Is.EqualTo(mockResponse.IbanEmpresa));
        Assert.That(responseValue.IbanCliente, Is.EqualTo(mockResponse.IbanCliente));
    }
}*/