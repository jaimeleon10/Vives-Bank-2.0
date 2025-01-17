using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Vives_Bank_Net.Rest.Producto.Cuenta.Controllers;
using Vives_Bank_Net.Rest.Producto.Cuenta.Dto;
using Vives_Bank_Net.Rest.Producto.Cuenta.Services;
using Vives_Bank_Net.Utils.Pagination;

namespace Vives_Bank_Net.Test.Rest.Producto.Cuenta.Controller;

public class CuentaAdminControllerTests
{
    private Mock<PaginationLinksUtils> _paginationLinks;
    private Mock<ILogger<CuentaControllerAdmin>> _mockLogger;
    private Mock<ICuentaService> _cuentaService;
    private CuentaControllerAdmin _cuentaController;

    [SetUp]
    public void SetUp()
    {
        _paginationLinks = new Mock<PaginationLinksUtils>();
        _mockLogger = new Mock<ILogger<CuentaControllerAdmin>>();
        _cuentaService = new Mock<ICuentaService>();
        _cuentaController = new CuentaControllerAdmin(_cuentaService.Object, _paginationLinks.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task GetAll()
    {
        var pageResponse = new PageResponse<CuentaResponse>
        {
            Content = new List<CuentaResponse>(),
            TotalElements = 10,
            TotalPages = 1,
            PageNumber = 0,
            PageSize = 10
        };
        _cuentaService.Setup(service => service.GetAll(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<PageRequest>()))
            .ReturnsAsync(pageResponse);

        _paginationLinks.Setup(utils => utils.CreateLinkHeader(It.IsAny<PageResponse<CuentaResponse>>(), It.IsAny<Uri>()))
            .Returns("<link>");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        _cuentaController.ControllerContext.HttpContext = httpContext;

        var result = await _cuentaController.Getall();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(result.Value, Is.TypeOf<PageResponse<CuentaResponse>>());
        Assert.That(result, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task GetAllByClientGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "guid",
            Iban = "ES12345678901234567890",
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 1,
            ProductoId = 1
        };
        var cuentas = new List<CuentaResponse> { cuenta };
        _cuentaService.Setup(service => service.getByClientGuid(It.IsAny<string>()))
            .ReturnsAsync(cuentas);

        var result = await _cuentaController.GetAllByClientGuid("123456789012");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<List<CuentaResponse>>());

        var cuentasResult = okResult.Value as List<CuentaResponse>;
        Assert.That(cuentasResult, Is.EqualTo(cuentas));
    }

    [Test]
    public async Task GetByGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "guid",
            Iban = "ES12345678901234567890",
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 1,
            ProductoId = 1
        };
        _cuentaService.Setup(service => service.getByGuid(It.IsAny<string>()))
            .ReturnsAsync(cuenta);

        var result = await _cuentaController.GetByGuid("123456789012");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<CuentaResponse>());
        
        var cuentaResult = okResult.Value as CuentaResponse;
        Assert.That(cuentaResult, Is.EqualTo(cuenta));
    }

    [Test]
    public async Task GetByIban()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "guid",
            Iban = "ES12345678901234567890",
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 1,
            ProductoId = 1
        };
        _cuentaService.Setup(service => service.getByIban(It.IsAny<string>()))
            .ReturnsAsync(cuenta);

        var result = await _cuentaController.GetByIban("ES12345678901234567890123456");
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.TypeOf<CuentaResponse>());
        
        var cuentaResult = okResult.Value as CuentaResponse;
        Assert.That(cuentaResult, Is.EqualTo(cuenta));
    }

    /*
    [Test]
    public async Task GetAll500()
    {
        _cuentaService.Setup(service => service.GetAll(It.IsAny<BigInteger?>(), It.IsAny<BigInteger?>(), It.IsAny<string>(), It.IsAny<PageRequest>()))
            .ThrowsAsync(new Exception("Test Exception"));

        var result = await _cuentaController.Getall();

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        Assert.That(result.StatusCode, Is.EqualTo(500));
    }
    */
}