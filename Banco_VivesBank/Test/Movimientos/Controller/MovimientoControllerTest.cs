using Banco_VivesBank.Movimientos.Controller;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Movimientos.Controller;

[TestFixture]
public class MovimientoControllerTests
{
    private Mock<IMovimientoService> _mockMovimientoService;
    private Mock<IUserService> _mockUserService; 
    private MovimientoController _controller;

    [SetUp]
    public void Setup()
    {
        _mockMovimientoService = new Mock<IMovimientoService>();
        _mockUserService = new Mock<IUserService>();  
        _controller = new MovimientoController(_mockMovimientoService.Object, _mockUserService.Object); 
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
    public async Task GetByGuid()
    {
        var guid = "guid";
        var mockMovimiento = new MovimientoResponse 
        { 
            Guid = guid, 
            ClienteGuid = "cliente1", 
            CreatedAt = "01/01/2025" 
        };

        _mockMovimientoService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync(mockMovimiento);
        
        var result = await _controller.GetByGuid(guid);
        
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
    public async Task GetByGuid_NotFound()
    {
        var guid = "non-existent-guid";
        var exceptionMessage = "No se ha encontrado el movimiento con guid: non-existent-guid";
        _mockMovimientoService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync((MovimientoResponse)null);
        
        var result = await _controller.GetByGuid(guid);
        
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
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
    public async Task GetByClienteGuid_ListaVacia()
    {
        var clienteGuid = "cliente1";
        var mockResponseList = new List<MovimientoResponse>();

        _mockMovimientoService.Setup(service => service.GetByClienteGuidAsync(clienteGuid))
            .ReturnsAsync(mockResponseList);
        var result = await _controller.GetByClienteGuid(clienteGuid);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);

        var responseList = okResult.Value as List<MovimientoResponse>;
        Assert.That(responseList, Is.Not.Null);
        Assert.That(responseList.Count, Is.EqualTo(0));
    }
    
    [Test]
    public async Task GetMyMovimientos()
    {
        var user = new Banco_VivesBank.User.Models.User { Id = 1, Username = "Test User" };
        _mockUserService.Setup(s => s.GetAuthenticatedUser()).Returns(user);

        var movimientos = new List<MovimientoResponse> 
        {
            new MovimientoResponse { Guid = "1", ClienteGuid = "clienteguid1", CreatedAt = DateTime.UtcNow.ToString() },
            new MovimientoResponse { Guid = "2", ClienteGuid = "clienteguid2", CreatedAt = DateTime.UtcNow.ToString() }
        };
        _mockMovimientoService.Setup(s => s.GetMyMovimientos(user)).ReturnsAsync(movimientos); 

        var result = await _controller.GetMyMovimientos();

        Assert.That(result, Is.InstanceOf<ActionResult<IEnumerable<MovimientoResponse>>>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));

        var returnedMovimientos = okResult.Value as IEnumerable<MovimientoResponse>;
        Assert.That(returnedMovimientos, Is.Not.Null);
        Assert.That(returnedMovimientos.Count(), Is.EqualTo(2));
    }
    
    [Test]
    public async Task GetMyMovimientosNotFound()
    {
        _mockUserService.Setup(s => s.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User)null);
    
        var result = await _controller.GetMyMovimientos();

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
    }
    
    [Test]
    public async Task CreateIngresoNomina()
    {
       
        var mockUser = new Banco_VivesBank.User.Models.User { Id = 1, Username = "user123", Password = "Usuario Test" };
        
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);

       
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "Empresa S.A.",
            CifEmpresa = "B12345678",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 1500.0
        };
        
        var mockResponse = new IngresoNominaResponse
        {
            NombreEmpresa = ingresoNominaRequest.NombreEmpresa,
            CifEmpresa = ingresoNominaRequest.CifEmpresa,
            IbanEmpresa = ingresoNominaRequest.IbanEmpresa,
            IbanCliente = ingresoNominaRequest.IbanCliente,
            Importe = ingresoNominaRequest.Importe
        };
        
        _mockMovimientoService.Setup(service => service.CreateIngresoNominaAsync(mockUser, ingresoNominaRequest))
            .ReturnsAsync(mockResponse);
        
        var result = await _controller.CreateIngresoNomina(ingresoNominaRequest);
        var okResult = result.Result as OkObjectResult;
        
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200)); 

        var responseValue = okResult.Value as IngresoNominaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.NombreEmpresa, Is.EqualTo(mockResponse.NombreEmpresa));
        Assert.That(responseValue.CifEmpresa, Is.EqualTo(mockResponse.CifEmpresa));
        Assert.That(responseValue.IbanEmpresa, Is.EqualTo(mockResponse.IbanEmpresa));
        Assert.That(responseValue.IbanCliente, Is.EqualTo(mockResponse.IbanCliente));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
    }
    
    [Test]
    public async Task CreateIngresoNomina_CuentaException()
    {
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "Empresa S.A.",
            CifEmpresa = "B12345678",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 1500.0
        };

        var exceptionMessage = "Cuenta no encontrada";

        var mockUser = new Banco_VivesBank.User.Models.User();
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);

        _mockMovimientoService
            .Setup(service => service.CreateIngresoNominaAsync(It.IsAny<Banco_VivesBank.User.Models.User>(), ingresoNominaRequest))
            .ThrowsAsync(new CuentaException(exceptionMessage));

        var result = await _controller.CreateIngresoNomina(ingresoNominaRequest);

        Console.WriteLine(result.Result?.GetType().Name);

        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null, "El resultado no es un NotFoundObjectResult");
        Assert.That(notFoundResult.Value, Is.Not.Null, "El valor del NotFoundObjectResult es nulo");

        var messageProperty = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value, null);

        Assert.That(messageProperty, Is.Not.Null, "El objeto no tiene una propiedad 'message'");
        Assert.That(messageProperty, Is.EqualTo(exceptionMessage), "El mensaje de error no coincide");
    }
   
    [Test]
    public async Task CreateIngresoNomina_MovimientoException()
    {
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "Empresa S.A.",
            CifEmpresa = "B12345678",
            IbanEmpresa = "ES1234567890",
            IbanCliente = "ES0987654321",
            Importe = 1500.0
        };

        var exceptionMessage = "Error en el movimiento";
    
        var mockUser = new Banco_VivesBank.User.Models.User();
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);

        _mockMovimientoService
            .Setup(service => service.CreateIngresoNominaAsync(It.IsAny<Banco_VivesBank.User.Models.User>(), ingresoNominaRequest))
            .ThrowsAsync(new MovimientoException(exceptionMessage));
    
        var result = await _controller.CreateIngresoNomina(ingresoNominaRequest);
    
        var badRequestResult = result.Result as BadRequestObjectResult;
    
        Assert.That(badRequestResult, Is.Not.Null, "El resultado no es un BadRequestObjectResult");
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>(), "El valor no es un objeto esperado");
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage), "El mensaje de error no coincide");
    }
    
    [Test]
    public async Task CreateIngresoNomina_Invalido()
    {
       
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "", 
            CifEmpresa = "", 
            IbanEmpresa = "ES123", 
            IbanCliente = "", 
            Importe = -1500.0 
        };

        _controller.ModelState.AddModelError("NombreEmpresa", "El nombre de la empresa es obligatorio");
        _controller.ModelState.AddModelError("CifEmpresa", "El CIF de la empresa es obligatorio");
        _controller.ModelState.AddModelError("IbanEmpresa", "El IBAN de la empresa no es válido");
        _controller.ModelState.AddModelError("IbanCliente", "El IBAN del cliente es obligatorio");
        _controller.ModelState.AddModelError("Importe", "El importe debe ser positivo");
        
        var result = await _controller.CreateIngresoNomina(ingresoNominaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.TypeOf<SerializableError>());
    }
    
    [Test]
    public async Task CreateTransferenciaCuentaInvalidaException()
    {
        var transferenciaRequest = new TransferenciaRequest(); 
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(new Banco_VivesBank.User.Models.User());
        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(It.IsAny<Banco_VivesBank.User.Models.User>(), It.IsAny<TransferenciaRequest>()))
            .ThrowsAsync(new CuentaInvalidaException("Cuenta inválida"));

        var result = await _controller.CreateTransferencia(transferenciaRequest);

        var actionResult = result.Result as BadRequestObjectResult;
        Assert.That(actionResult, Is.Not.Null);
        Assert.That(actionResult.Value, Is.InstanceOf<object>());
        Assert.That(actionResult.Value.ToString(), Does.Contain("Cuenta inválida"));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "Tienda S.A.",
            Importe = 100.0,
            NumeroTarjeta = "4111111111111111"
        };

        var mockResponse = new PagoConTarjetaResponse
        {
            NombreComercio = pagoConTarjetaRequest.NombreComercio,
            Importe = pagoConTarjetaRequest.Importe,
            NumeroTarjeta = pagoConTarjetaRequest.NumeroTarjeta
        };
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);

        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(mockUser, pagoConTarjetaRequest))
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
            NombreComercio = "Tienda S.A.",
            Importe = 100.0,
            NumeroTarjeta = "4111111111111111"
        };

        var exceptionMessage = "Tarjeta no válida";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        
        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(mockUser, pagoConTarjetaRequest))
            .ThrowsAsync(new TarjetaException(exceptionMessage));

        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_SaldoCuentaInsuficientException()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "Tienda S.A.",
            Importe = 100.0,
            NumeroTarjeta = "4111111111111111"
        };

        var exceptionMessage = "Saldo insuficiente";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        
        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(mockUser, pagoConTarjetaRequest))
            .ThrowsAsync(new SaldoCuentaInsuficientException(exceptionMessage));

        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_CuentaException()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "Tienda S.A.",
            Importe = 100.0,
            NumeroTarjeta = "4111111111111111"
        };

        var exceptionMessage = "Cuenta no encontrada";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(mockUser, pagoConTarjetaRequest))
            .ThrowsAsync(new CuentaException(exceptionMessage));

        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_MovimientoException()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "Tienda S.A.",
            Importe = 100.0,
            NumeroTarjeta = "4111111111111111"
        };

        var exceptionMessage = "Error en el movimiento";
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.CreatePagoConTarjetaAsync(mockUser, pagoConTarjetaRequest))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreatePagoConTarjeta_Invalido()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "", 
            Importe = -100.0,
            NumeroTarjeta = "123" 
        };

        _controller.ModelState.AddModelError("NombreComercio", "El nombre del comercio es obligatorio");
        _controller.ModelState.AddModelError("Importe", "El importe debe ser un número positivo");
        _controller.ModelState.AddModelError("NumeroTarjeta", "El número de tarjeta no es válido");
        
        var result = await _controller.CreatePagoConTarjeta(pagoConTarjetaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.TypeOf<SerializableError>());
    }
    
    [Test]
    public async Task CreateTransferencia()
    {
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "ES9121000418450200051332",
            IbanDestino = "ES4721000418450200051333",
            NombreBeneficiario = "Juan Pérez",
            Importe = 100.0
        };

        var mockResponse = new TransferenciaResponse
        {
            IbanOrigen = transferenciaRequest.IbanOrigen,
            IbanDestino = transferenciaRequest.IbanDestino,
            NombreBeneficiario = transferenciaRequest.NombreBeneficiario,
            Importe = transferenciaRequest.Importe,
            Revocada = false
        };
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        
        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(mockUser, transferenciaRequest))
            .ReturnsAsync(mockResponse);

        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var responseValue = okResult.Value as TransferenciaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.IbanOrigen, Is.EqualTo(mockResponse.IbanOrigen));
        Assert.That(responseValue.IbanDestino, Is.EqualTo(mockResponse.IbanDestino));
        Assert.That(responseValue.NombreBeneficiario, Is.EqualTo(mockResponse.NombreBeneficiario));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.Revocada, Is.False);
    }
    
    [Test]
    public async Task CreateTransferencia_SaldoCuentaInsuficientException()
    {
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "ES9121000418450200051332",
            IbanDestino = "ES4721000418450200051333",
            NombreBeneficiario = "Juan Pérez",
            Importe = 1000.0
        };

        var exceptionMessage = "Saldo insuficiente";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(mockUser, transferenciaRequest))
            .ThrowsAsync(new SaldoCuentaInsuficientException(exceptionMessage));

        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreateTransferencia_CuentaException()
    {
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "ES9121000418450200051332",
            IbanDestino = "ES4721000418450200051333",
            NombreBeneficiario = "Juan Pérez",
            Importe = 100.0
        };

        var exceptionMessage = "Cuenta no encontrada";
       
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(mockUser, transferenciaRequest))
            .ThrowsAsync(new CuentaException(exceptionMessage));

        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreateTransferencia_MovimientoException()
    {
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "ES9121000418450200051332",
            IbanDestino = "ES4721000418450200051333",
            NombreBeneficiario = "Juan Pérez",
            Importe = 100.0
        };

        var exceptionMessage = "Error al procesar la transferencia";
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.CreateTransferenciaAsync(mockUser, transferenciaRequest))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task CreateTransferencia_Invalido()
    {
     
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "", 
            IbanDestino = "",
            NombreBeneficiario = "Juan Pérez",
            Importe = -100.0 
        };

        _controller.ModelState.AddModelError("IbanOrigen", "El IBAN de origen es obligatorio");
        _controller.ModelState.AddModelError("IbanDestino", "El IBAN de destino es obligatorio");
        _controller.ModelState.AddModelError("Importe", "El importe debe ser un número positivo");
        
        var result = await _controller.CreateTransferencia(transferenciaRequest);
        var badRequestResult = result.Result as BadRequestObjectResult;
        
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.TypeOf<SerializableError>());
    }
    
    [Test]
    public async Task RevocarTransferencia()
    {
        var movimientoGuid = "movimiento1";

        var mockResponse = new TransferenciaResponse
        {
            IbanOrigen = "ES9121000418450200051332",
            IbanDestino = "ES4721000418450200051333",
            NombreBeneficiario = "Juan Pérez",
            Importe = 100.0,
            Revocada = true
        };

       
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(mockUser, movimientoGuid))
            .ReturnsAsync(mockResponse);

        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);

        var responseValue = okResult.Value as TransferenciaResponse;
        Assert.That(responseValue, Is.Not.Null);
        Assert.That(responseValue.IbanOrigen, Is.EqualTo(mockResponse.IbanOrigen));
        Assert.That(responseValue.IbanDestino, Is.EqualTo(mockResponse.IbanDestino));
        Assert.That(responseValue.NombreBeneficiario, Is.EqualTo(mockResponse.NombreBeneficiario));
        Assert.That(responseValue.Importe, Is.EqualTo(mockResponse.Importe));
        Assert.That(responseValue.Revocada, Is.True);
    }

    [Test]
    public async Task RevocarTransferencia_MovimientoNotFoundException()
    {
        var movimientoGuid = "movimiento1";
        var exceptionMessage = "Movimiento no encontrado";
        
        var mockUser = new Banco_VivesBank.User.Models.User{  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(mockUser, movimientoGuid))
            .ThrowsAsync(new MovimientoNotFoundException(exceptionMessage));

        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task RevocarTransferencia_MovimientoException()
    {
        var movimientoGuid = "movimiento1";
        var exceptionMessage = "Error al revocar el movimiento";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(mockUser, movimientoGuid))
            .ThrowsAsync(new MovimientoException(exceptionMessage));

        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));    
    }
    
    [Test]
    public async Task RevocarTransferencia_SaldoCuentaInsuficientException()
    {
        var movimientoGuid = "movimiento1";
        var exceptionMessage = "Saldo insuficiente para revocar la transferencia";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(mockUser, movimientoGuid))
            .ThrowsAsync(new SaldoCuentaInsuficientException(exceptionMessage));

        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var badRequestResult = result.Result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.InstanceOf<object>());
        Assert.That(badRequestResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
    
    [Test]
    public async Task RevocarTransferencia_CuentaException()
    {
        var movimientoGuid = "movimiento1";
        var exceptionMessage = "Cuenta no encontrada";
        
        var mockUser = new Banco_VivesBank.User.Models.User {  };
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(mockUser);
        _mockMovimientoService.Setup(service => service.RevocarTransferenciaAsync(mockUser, movimientoGuid))
            .ThrowsAsync(new CuentaException(exceptionMessage));

        var result = await _controller.RevocarTransferencia(movimientoGuid);
        var notFoundResult = result.Result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.InstanceOf<object>());
        Assert.That(notFoundResult.Value.ToString(), Does.Contain(exceptionMessage));
    }
}