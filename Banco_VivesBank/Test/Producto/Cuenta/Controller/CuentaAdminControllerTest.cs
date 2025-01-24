using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Controllers;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Test.Producto.Cuenta.Controller;

public class CuentaAdminControllerTests
{
    private Mock<PaginationLinksUtils> _paginationLinks;
    private Mock<ICuentaService> _cuentaService;
    private CuentaControllerAdmin _cuentaController;

    [SetUp]
    public void SetUp()
    {
        _paginationLinks = new Mock<PaginationLinksUtils>();
        _cuentaService = new Mock<ICuentaService>();
        _cuentaController = new CuentaControllerAdmin(_cuentaService.Object, _paginationLinks.Object);
    }
    
    [Test]
    public async Task GetAll()
    {
        var expectedResponse = new PageResponse<CuentaResponse>
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "id",
            Direction = "asc",
        };

        _cuentaService
            .Setup(service => service.GetAllAsync(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<PageRequest>()))
            .ReturnsAsync(expectedResponse);

        var baseUri = new Uri("https://localhost");
        _paginationLinks
            .Setup(utils => utils.CreateLinkHeader(It.IsAny<PageResponse<CuentaResponse>>(), baseUri))
            .Returns("link-header");

        _cuentaController.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await _cuentaController.Getall(saldoMax: null, saldoMin: null, tipoCuenta: null, page: 0, size: 10, sortBy: "id", direction: "asc");

        Assert.That(result, Is.TypeOf<ActionResult<PageResponse<CuentaResponse>>>());
    }
    
    [Test]
    public async Task GetAllNotFound()
    {
        _cuentaService.Setup(service => service.GetAllAsync(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<PageRequest>()))
            .ThrowsAsync(new CuentaNotFoundException("No se han encontrado las cuentas."));

        var result = await _cuentaController.Getall(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<int>());
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task GetAll500()
    {
        _cuentaService.Setup(service => service.GetAllAsync(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<PageRequest>()))
            .ThrowsAsync(new Exception("Ocurrió un error procesando la solicitud."));

        var result = await _cuentaController.Getall(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<int>());
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetAllByClientGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };
        var cuentas = new List<CuentaResponse> { cuenta };
        _cuentaService.Setup(service => service.GetByClientGuidAsync(It.IsAny<string>()))
            .ReturnsAsync(cuentas);

        var result = await _cuentaController.GetAllByClientGuid("cuenta-guid");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<List<CuentaResponse>>());

        var cuentasResult = okResult.Value as List<CuentaResponse>;
        Assert.That(cuentasResult, Is.EqualTo(cuentas));
    }
    
    [Test]
    public async Task GetAllByClienteGuidBadRequest()
    {
        _cuentaService.Setup(service => service.GetByClientGuidAsync("???"))
            .ThrowsAsync(new CuentaInvalidaException("Cuentas no encontradas por guid del cliente invalido."));

        var result = await _cuentaController.GetAllByClientGuid("???");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
    }
    
    [Test]
    public async Task GetAllByClienteGuidNotFound()
    {
        _cuentaService.Setup(service => service.GetByClientGuidAsync("cliente-Guid"))
            .ThrowsAsync(new CuentaNotFoundException("No se han encontrado las cuentas del cliente."));

        var result = await _cuentaController.GetAllByClientGuid("clienteGuid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task GetAllByClienteGuid500()
    {
        _cuentaService.Setup(service => service.GetByClientGuidAsync("clienteGuid"))
            .ThrowsAsync(new Exception("Ocurrió un error procesando la solicitud."));

        var result = await _cuentaController.GetAllByClientGuid("clienteGuid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetByGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };
        _cuentaService.Setup(service => service.GetByGuidAsync(It.IsAny<string>()))
            .ReturnsAsync(cuenta);

        var result = await _cuentaController.GetByGuid("123456789012");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<CuentaResponse>());
        
        var cuentaResult = okResult.Value as CuentaResponse;
        Assert.That(cuentaResult, Is.EqualTo(cuenta));
    }
    
    [Test]
    public async Task GetByGuidBadRequest()
    {
        _cuentaService.Setup(service => service.GetByGuidAsync("???"))
            .ThrowsAsync(new CuentaInvalidaException("Cuenta no encontrada por guid invalido."));

        var result = await _cuentaController.GetByGuid("???");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
    }
    
    [Test]
    public async Task GetByGuidNotFound()
    {
        _cuentaService.Setup(service => service.GetByGuidAsync("guid"))
            .ThrowsAsync(new CuentaNotFoundException("No se ha encontrado la cuenta."));

        var result = await _cuentaController.GetByGuid("guid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task GetByGuid500()
    {
        _cuentaService.Setup(service => service.GetByGuidAsync("guid"))
            .ThrowsAsync(new Exception("Ocurrió un error procesando la solicitud."));

        var result = await _cuentaController.GetByGuid("guid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetByIban()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };
        _cuentaService.Setup(service => service.GetByIbanAsync(It.IsAny<string>()))
            .ReturnsAsync(cuenta);

        var result = await _cuentaController.GetByIban("ES12345678901234567890123456");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<CuentaResponse>());
        
        var cuentaResult = okResult.Value as CuentaResponse;
        Assert.That(cuentaResult, Is.EqualTo(cuenta));
    }
    
    [Test]
    public async Task GetByIbanBadRequest()
    {
        _cuentaService.Setup(service => service.GetByIbanAsync("???"))
            .ThrowsAsync(new CuentaInvalidaException("Cuenta no encontrada por iban invalido."));

        var result = await _cuentaController.GetByIban("???");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
    }
    
    [Test]
    public async Task GetByIbanNotFound()
    {
        _cuentaService.Setup(service => service.GetByIbanAsync("iban"))
            .ThrowsAsync(new CuentaNotFoundException("No se ha encontrado la cuenta."));

        var result = await _cuentaController.GetByIban("iban");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task GetByIban500()
    {
        _cuentaService.Setup(service => service.GetByIbanAsync("iban"))
            .ThrowsAsync(new Exception("Ocurrió un error procesando la solicitud."));

        var result = await _cuentaController.GetByIban("iban");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    [Test]
    public async Task Delete()
    {
        var guid = "cuenta-guid";
        var expectedResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            TarjetaGuid = null,
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsDeleted = false
        };

        _cuentaService.Setup(service => service.DeleteAdminAsync(guid))
            .ReturnsAsync(expectedResponse);

        var result = await _cuentaController.Delete(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var cuentaResponse = okResult.Value as CuentaResponse;

        Assert.That(cuentaResponse, Is.Not.Null);
        Assert.That(cuentaResponse.Guid, Is.EqualTo(expectedResponse.Guid));
        Assert.That(cuentaResponse.Iban, Is.EqualTo(expectedResponse.Iban));
    }
    
    [Test]
    public async Task DeleteNotFound()
    {
        _cuentaService.Setup(service => service.DeleteAdminAsync("guid"))
            .ThrowsAsync(new CuentaNotFoundException("No se ha encontrado la cuenta."));

        var result = await _cuentaController.Delete("guid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task Delete500()
    {
        _cuentaService.Setup(service => service.DeleteAdminAsync("guid"))
            .ThrowsAsync(new Exception("Ocurrió un error procesando la solicitud."));

        var result = await _cuentaController.Delete("guid");
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
}