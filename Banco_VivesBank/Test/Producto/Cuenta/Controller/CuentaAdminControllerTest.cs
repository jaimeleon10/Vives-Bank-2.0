using System.Numerics;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Controllers;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
        var expectedCuentas = new List<CuentaResponse>
        {
            new CuentaResponse { Guid = "guid1", Iban = "Producto1", Saldo = "3000" },
            new CuentaResponse { Guid = "guid2", Iban = "Producto2", Saldo = "3000" }
        };

        var page = 0;
        var size = 10;
        var sortBy = "id";
        var direction = "desc";
        
        var pageRequest = new PageRequest
        {
            PageNumber = page,
            PageSize = size,
            SortBy = sortBy,
            Direction = direction
        };

        var pageResponse = new PageResponse<CuentaResponse>
        {
            Content = expectedCuentas,
            TotalElements = expectedCuentas.Count,
            PageNumber = pageRequest.PageNumber,
            PageSize = pageRequest.PageSize,
            TotalPages = 1
        };

        _cuentaService.Setup(s => s.GetAllAsync(5000, 1000, "normal", pageRequest))
            .ReturnsAsync(pageResponse);

        var baseUri = new Uri("http://localhost/api/cuentas");
        _paginationLinks.Setup(utils => utils.CreateLinkHeader(pageResponse, baseUri))
            .Returns("<http://localhost/api/productosBase?page=0&size=5>; rel=\"prev\",<http://localhost/api/cuentas?page=2&size=5>; rel=\"next\"");

        // Configurar el contexto HTTP para la prueba
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/cuentas")
            }
        };

        _cuentaController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var result = await _cuentaController.Getall(page, size, sortBy);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task GetAllByClientGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
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
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        
        var objectResult = result.Result as BadRequestObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
    }
    
    [Test]
    public async Task GetAllByClienteGuidNotFound()
    {
        _cuentaService.Setup(service => service.GetByClientGuidAsync("cliente-GuidNoexistente"))
            .ThrowsAsync(new ClienteNotFoundException("Error buscando cliente."));

        var result = await _cuentaController.GetAllByClientGuid("cliente-GuidNoexistente");
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        
        var objectResult = result.Result as NotFoundObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetByGuid()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
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
    public async Task GetByGuidNotFound()
    {
        _cuentaService.Setup(service => service.GetByGuidAsync("guid"))
            .ReturnsAsync((CuentaResponse)null);

        var result = await _cuentaController.GetByGuid("guid");
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        
        var objectResult = result.Result as NotFoundObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetByIban()
    {
        var cuenta = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
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
    public async Task GetByIbanNotFound()
    {
        _cuentaService.Setup(service => service.GetByIbanAsync("iban"))
            .ReturnsAsync((CuentaResponse)null);

        var result = await _cuentaController.GetByIban("iban");
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        
        var objectResult = result.Result as NotFoundObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
    
    [Test]
    public async Task Delete()
    {
        var guid = "cuenta-guid";
        var expectedResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "ES1234567890123456789012",
            Saldo = "1000",
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
            .ReturnsAsync((CuentaResponse)null);

        var result = await _cuentaController.Delete("guid");
        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        
        var objectResult = result.Result as NotFoundObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
}