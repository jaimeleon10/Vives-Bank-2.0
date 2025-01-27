using Banco_VivesBank.Movimientos.Controller;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Movimientos.Controller;

[TestFixture]
public class MovimientoControllerTests
{
    private Mock<IMovimientoService> _mockMovimientoService;
    private MovimientoController _controller;

    [SetUp]
    public void Setup()
    {
        _mockMovimientoService = new Mock<IMovimientoService>();
        _controller = new MovimientoController(_mockMovimientoService.Object);
    }
    
    [Test]
    public async Task GetAll()
    {
     
        var mockResponseList = new List<MovimientoResponse>
        {
            new MovimientoResponse { Guid = "1", ClienteGuid = "cliente1", CreatedAt = "01/01/2025" },
            new MovimientoResponse { Guid = "2", ClienteGuid = "cliente2", CreatedAt = "02/01/2025" }
        };

        _mockMovimientoService.Setup(service => service.GetAllAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<MovimientoResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockResponseList.Count));
        
        foreach (var response in responseList)
        {
            Assert.That(response.CreatedAt, Is.Not.Null);
        }
    }

    [Test]
    public async Task GetAll_NoExisteMovimiento()
    {
      
        var mockResponseList = new List<MovimientoResponse>();

        _mockMovimientoService.Setup(service => service.GetAllAsync())
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetAll();
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<MovimientoResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(0)); // Esperamos que la lista esté vacía
    }
    
    [Test]
    public async Task GetById()
    {
        var guid = "1";
        var mockMovimiento = new MovimientoResponse 
        { 
            Guid = guid, 
            ClienteGuid = "cliente1", 
            CreatedAt = "01/01/2025" 
        };

        _mockMovimientoService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync(mockMovimiento);
        
        var result = await _controller.GetById(guid);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    
        var responseValue = okResult.Value as MovimientoResponse;
        Assert.That(responseValue, Is.Not.Null);
    
        Assert.That(responseValue.Guid, Is.EqualTo(mockMovimiento.Guid));
        Assert.That(responseValue.ClienteGuid, Is.EqualTo(mockMovimiento.ClienteGuid));
        Assert.That(responseValue.CreatedAt, Is.EqualTo(mockMovimiento.CreatedAt));
        
        Assert.That(responseValue.Domiciliacion, Is.Null);
        Assert.That(responseValue.IngresoNomina, Is.Null);
        Assert.That(responseValue.PagoConTarjeta, Is.Null);
        Assert.That(responseValue.Transferencia, Is.Null);
    }
    
    [Test]
    public async Task GetById_MovimientoNotFound()
    {
        var guid = "non-existent-guid";
        _mockMovimientoService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync((MovimientoResponse)null);
        
        var result = await _controller.GetById(guid);
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha encontrado el movimiento con guid: {guid}"));
    }
    
    [Test]
    public async Task GetByClienteGuid()
    {
        var clienteGuid = "cliente1";
        var mockResponseList = new List<MovimientoResponse>
        {
            new MovimientoResponse { Guid = "1", ClienteGuid = clienteGuid, CreatedAt = "01/01/2025" },
            new MovimientoResponse { Guid = "2", ClienteGuid = clienteGuid, CreatedAt = "02/01/2025" }
        };

        _mockMovimientoService.Setup(service => service.GetByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetByClienteGuid(clienteGuid);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    
        var responseList = okResult.Value as List<MovimientoResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(mockResponseList.Count));
        
        foreach (var response in responseList)
        {
            Assert.That(response.ClienteGuid, Is.EqualTo(clienteGuid));
        }
    }
    
    [Test]
    public async Task GetByClienteGuid_NoMovimientosFound()
    {
        var clienteGuid = "cliente1";
        var mockResponseList = new List<MovimientoResponse>(); 

        _mockMovimientoService.Setup(service => service.GetByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(mockResponseList);
        
        var result = await _controller.GetByClienteGuid(clienteGuid);
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha encontrado el movimiento con clienteGuid: {clienteGuid}"));
    }
    
    [Test]
    public async Task GetAllDomiciliaciones()
    {
        // Arrange
        var mockResponseList = new List<DomiciliacionResponse>
        {
            new DomiciliacionResponse { Guid = "1", ClienteGuid = "cliente1", Importe = 100.0, Periodicidad = "Mensual", Activa = true, FechaInicio = "01/01/2025", UltimaEjecuccion = "01/02/2025" },
            new DomiciliacionResponse { Guid = "2", ClienteGuid = "cliente2", Importe = 200.0, Periodicidad = "Mensual", Activa = false, FechaInicio = "02/01/2025", UltimaEjecuccion = "02/02/2025" }
        };

        _mockMovimientoService.Setup(service => service.GetAllDomiciliacionesAsync())
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

        _mockMovimientoService.Setup(service => service.GetAllDomiciliacionesAsync())
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

    _mockMovimientoService.Setup(service => service.GetDomiciliacionesByClienteGuidAsync(clienteGuid))
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

        _mockMovimientoService.Setup(service => service.GetDomiciliacionesByClienteGuidAsync(clienteGuid))
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
        
        _mockMovimientoService.Setup(service => service.CreateDomiciliacionAsync(domiciliacionRequest))
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
    
    [Test]
    public async Task CreateIngresoNomina()
    {
        var ingresoNominaRequest = new IngresoNominaRequest { };
        
        var mockResponse = new IngresoNominaResponse
        {
            NombreEmpresa = "EmpresaTest", 
            CifEmpresa = "A12345678", 
            IbanEmpresa = "ES1234567890123456789012",  
            IbanCliente = "ES9876543210987654321098", 
            Importe = 1000.0  
        };
        
        _mockMovimientoService.Setup(service => service.CreateIngresoNominaAsync(ingresoNominaRequest))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.CreateIngresoNomina(ingresoNominaRequest);
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var responseValue = okResult.Value as IngresoNominaResponse;
        
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.NombreEmpresa, Is.EqualTo(mockResponse.NombreEmpresa));
        Assert.That(responseValue.CifEmpresa, Is.EqualTo(mockResponse.CifEmpresa));
        Assert.That(responseValue.IbanEmpresa, Is.EqualTo(mockResponse.IbanEmpresa));
        Assert.That(responseValue.IbanCliente, Is.EqualTo(mockResponse.IbanCliente));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest 
        { 
            NombreComercio = "Comercio XYZ", 
            Importe = 100.0, 
            NumeroTarjeta = "1234-5678-9876-5432" 
        };

        var mockResponse = new PagoConTarjetaResponse 
        { 
            NombreComercio = "Comercio XYZ",
            Importe = 100.0, 
            NumeroTarjeta = "1234-5678-9876-5432"
        };

        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(pagoConTarjetaRequest))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);
    
        var responseValue = okResult.Value as PagoConTarjetaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.NombreComercio, Is.EqualTo(mockResponse.NombreComercio));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.NumeroTarjeta, Is.EqualTo(mockResponse.NumeroTarjeta));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_TarjetaException()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest 
        { 
            NombreComercio = "Comercio XYZ", 
            Importe = 100.0, 
            NumeroTarjeta = "1234-5678-9876-5432"
        };
        var exceptionMessage = "Tarjeta no válida";

        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(pagoConTarjetaRequest))
            .ThrowsAsync(new TarjetaException(exceptionMessage));
        
        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_CuentaException()
    {
       
        var pagoConTarjetaRequest = new PagoConTarjetaRequest 
        { 
            NombreComercio = "Comercio XYZ", 
            Importe = 100.0, 
            NumeroTarjeta = "1234-5678-9876-5432"
        };
        var exceptionMessage = "Cuenta no encontrada";

        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(pagoConTarjetaRequest))
            .ThrowsAsync(new CuentaException(exceptionMessage));
        
        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    
    [Test]
    public async Task CreatePagoConTarjeta_Invalido()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest(); 
        _controller.ModelState.AddModelError("NombreComercio", "El nombre del comercio es un campo obligatorio"); 
        
        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(_controller.ModelState)); 
    }
    
    
    [Test]
    public async Task CreateTransferencia()
    {
        var transferenciaRequest = new TransferenciaRequest 
        { 
            IbanOrigen = "ES1234567890", 
            IbanDestino = "ES0987654321", 
            Importe = 100.0 
        };
        var mockResponse = new TransferenciaResponse 
        { 
            IbanOrigen = "ES1234567890", 
            NombreBeneficiario = "Juan Pérez", 
            IbanDestino = "ES0987654321", 
            Importe = 100.0, 
            Revocada = false 
        };

        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(transferenciaRequest))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    
        var responseValue = okResult.Value as TransferenciaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.IbanOrigen, Is.EqualTo(mockResponse.IbanOrigen));
        Assert.That(responseValue.NombreBeneficiario, Is.EqualTo(mockResponse.NombreBeneficiario));
        Assert.That(responseValue.IbanDestino, Is.EqualTo(mockResponse.IbanDestino));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.Revocada, Is.EqualTo(mockResponse.Revocada));
    }
    
    [Test]
    public async Task CreateTransferencia_CuentaException()
    {
       
        var transferenciaRequest = new TransferenciaRequest 
        { 
            IbanOrigen = "ES1234567890", 
            IbanDestino = "ES0987654321", 
            Importe = 100.0 
        };
        var exceptionMessage = "Cuenta no encontrada";

        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(transferenciaRequest))
            .ThrowsAsync(new CuentaException(exceptionMessage));
        
        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    
    [Test]
    public async Task CreateTransferencia_Invalido()
    {
        var transferenciaRequest = new TransferenciaRequest(); 
        _controller.ModelState.AddModelError("IbanOrigen", "El campo es obligatorio"); 
        
        var result = await _controller.CreateTransferencia(transferenciaRequest);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(_controller.ModelState)); 
    }
    [Test]
    public async Task RevocarTransferencia()
    {
        var movimientoGuid = "1";
        var mockResponse = new TransferenciaResponse 
        { 
            IbanOrigen = "ES1234567890123456789012", 
            NombreBeneficiario = "BeneficiarioTest", 
            IbanDestino = "ES9876543210987654321098",  
            Importe = 1000.0,  
            Revocada = true  
        };
        
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(movimientoGuid))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseValue = okResult.Value as TransferenciaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.IbanOrigen, Is.EqualTo(mockResponse.IbanOrigen));
        Assert.That(responseValue.NombreBeneficiario, Is.EqualTo(mockResponse.NombreBeneficiario));
        Assert.That(responseValue.IbanDestino, Is.EqualTo(mockResponse.IbanDestino));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.Revocada, Is.EqualTo(mockResponse.Revocada));
    }
    
    [Test]
    public async Task RevocarTransferencia_CuentaException()
    {
        var movimientoGuid = "1";
        var exceptionMessage = "Cuenta no encontrada";

        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(movimientoGuid))
            .ThrowsAsync(new CuentaException(exceptionMessage));
        
        var result = await _controller.RevocarTransferencia(movimientoGuid);
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    [Test]
    public async Task RevocarTransferencia_MovimientoException()
    {
        var movimientoGuid = "1";
        var exceptionMessage = "Error en el movimiento";

        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(movimientoGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));
        
        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.EqualTo(exceptionMessage));
    }
    
    [Test]
    public async Task RevocarTransferencia_TransferenciaNotFound()
    {
     
        var movimientoGuid = "1"; 
        var exceptionMessage = "Transferencia no encontrada";

        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(movimientoGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));
        
        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var notFoundResult = result.Result as NotFoundObjectResult;
        
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo(exceptionMessage));
    }
    
}