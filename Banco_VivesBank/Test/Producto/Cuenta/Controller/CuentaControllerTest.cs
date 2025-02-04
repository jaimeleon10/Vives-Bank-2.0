using System.Numerics;
using System.Security.Claims;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Controllers;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Producto.Cuenta.Controller;

public class CuentaAdminControllerTests
{
    private  Mock<ICuentaService> _mockCuentaService;
    private  Mock<PaginationLinksUtils> _mockPaginationLinksUtils;
    private  Mock<IUserService> _mockUserService;
    private  CuentaController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockCuentaService = new Mock<ICuentaService>();
        _mockPaginationLinksUtils = new Mock<PaginationLinksUtils>();
        _mockUserService = new Mock<IUserService>();
        _controller = new CuentaController(_mockCuentaService.Object, _mockPaginationLinksUtils.Object, _mockUserService.Object);
    }
    
   [Test]
    public async Task GetAllOk()
    {
        var expectedCuentas = new List<CuentaResponse>
        {
            new CuentaResponse
            {
                Guid = "guid1",
                Iban = "ES9121000418450200051332",
                Saldo = 1500.50,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = "cliente-guid",
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            },
            new CuentaResponse
            {
                Guid = "guid2",
                Iban = "ES9121000418450200051332",
                Saldo = 1500.50,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = "cliente-guid",
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            }
        };

        var page = 0;
        var size = 10;
        var sortBy = "Id";
        var direction = "desc";
        double? saldoMax = 5000;
        double? saldoMin = 1000;
        string tipoCuenta = "normal";

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

        _mockCuentaService.Setup(s => s.GetAllAsync(saldoMax, saldoMin, tipoCuenta, pageRequest))
            .ReturnsAsync(pageResponse);

        var baseUri = new Uri("http://localhost/api/cuentas/admin");  // Asegúrate de incluir /admin
        _mockPaginationLinksUtils.Setup(utils => utils.CreateLinkHeader(pageResponse, baseUri))
            .Returns("<http://localhost/api/cuentas/admin?page=0&size=10>; rel=\"prev\",<http://localhost/api/cuentas/admin?page=2&size=10>; rel=\"next\"");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminUser"),
            new Claim(ClaimTypes.Role, "Admin") 
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/cuentas"), 
                Path = new PathString("/admin") 
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };


        var result = await _controller.Getall(saldoMax, saldoMin, tipoCuenta, page, size, sortBy, direction);

        Assert.That(result, Is.Not.Null);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
    }
    
    [Test]
    public async Task GetAllByClientGuiOk()
    {
        var guid = "test-guid";
        var expectedCuentas = new List<CuentaResponse>
        {
            new CuentaResponse
            {
                Guid = "cuenta1",
                Iban = "ES9121000418450200051332",
                Saldo = 1500.50,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = guid,
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            },
            new CuentaResponse
            {
                Guid = "cuenta2",
                Iban = "ES9121000418450200051332",
                Saldo = 1200.75,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = guid,
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            }
        };

        _mockCuentaService.Setup(service => service.GetByClientGuidAsync(guid))
            .ReturnsAsync(expectedCuentas);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/cuentas"),  
                Path = new PathString("/admin/cliente/" + guid) 
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetAllByClientGuid(guid);

        Assert.That(result, Is.Not.Null);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var returnValue = okResult.Value as List<CuentaResponse>;
        Assert.That(returnValue, Is.Not.Null);
        Assert.That(returnValue.Count, Is.EqualTo(expectedCuentas.Count));
        Assert.That(returnValue, Is.EquivalentTo(expectedCuentas));
    }
    
    [Test]
    public async Task GetAllByClientGuidNotFound()
    {
        var guid = "non-existent-guid";

        _mockCuentaService.Setup(service => service.GetByClientGuidAsync(guid))
            .ThrowsAsync(new ClienteNotFoundException("Cliente no encontrado"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/cuentas"), 
                Path = new PathString("/admin/" + guid)  
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetAllByClientGuid(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());

        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
    }
    
    [Test]
    public async Task GetAllByClientGuid_ReturnsInternalServerError_WhenUnexpectedErrorOccurs()
    {
        var guid = "test-guid";

        _mockCuentaService.Setup(service => service.GetByClientGuidAsync(guid))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/cuentas"), 
                Path = new PathString("/admin/" + guid) 
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetAllByClientGuid(guid);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());

        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        
    }

    
    [Test]
    public async Task GetByGuiOk()
    {
        var guid = "test-guid";
        var expectedCuenta = new CuentaResponse
        {
            Guid = guid,
            Iban = "ES9121000418450200051332",
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockCuentaService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync(expectedCuenta);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal 
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

    }
    [Test]
    public async Task GetByGuidNotFound()
    {
        var guid = "non-existing-guid";

        _mockCuentaService.Setup(service => service.GetByGuidAsync(guid))
            .ReturnsAsync((CuentaResponse)null);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);

    }
    
    [Test]
    public async Task GetByGuid_ReturnsInternalServerError_WhenAnExceptionOccurs()
    {
        var guid = "test-guid";

        _mockCuentaService.Setup(service => service.GetByGuidAsync(guid))
            .ThrowsAsync(new Exception("Error interno"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    
    [Test]
    public async Task GetByIbaOk()
    {
        var iban = "ES9121000418450200051332";
        var expectedCuenta = new CuentaResponse
        {
            Guid = "test-guid",
            Iban = iban,
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockCuentaService.Setup(service => service.GetByIbanAsync(iban))
            .ReturnsAsync(expectedCuenta);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByIban(iban);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
    }

    [Test]
    public async Task GetByIbanNotFound()
    {
        var iban = "ES9121000418450200051332";

        _mockCuentaService.Setup(service => service.GetByIbanAsync(iban))
            .ReturnsAsync((CuentaResponse)null);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByIban(iban);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);

    }
    
    [Test]
    public async Task GetByIbanException()
    {
        var iban = "ES9121000418450200051332";

        _mockCuentaService.Setup(service => service.GetByIbanAsync(iban))
            .ThrowsAsync(new Exception("Something went wrong"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await _controller.GetByIban(iban);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult.StatusCode, Is.EqualTo(500));

    }
    
    [Test]
    public async Task DeleteAdminOk()
    {
        var guid = "test-guid";
        var expectedCuenta = new CuentaResponse
        {
            Guid = guid,
            Iban = "ES9121000418450200051332",
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockCuentaService.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync(expectedCuenta);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.DeleteAdmin(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAdminNotFound()
    {
        var guid = "non-existent-guid";

        _mockCuentaService.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync((CuentaResponse)null);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.DeleteAdmin(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }
    
    [Test]
    public async Task DeleteAdminException()
    {
        var guid = "test-guid";

        _mockCuentaService.Setup(service => service.DeleteByGuidAsync(guid))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.DeleteAdmin(guid);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetAllMeAccountsOk()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var expectedCuentas = new List<CuentaResponse>
        {
            new CuentaResponse
            {
                Guid = "guid1",
                Iban = "ES9121000418450200051332",
                Saldo = 1500.50,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = "cliente-guid",
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            },
            new CuentaResponse
            {
                Guid = "guid2",
                Iban = "ES9121000418450200051333",
                Saldo = 1500.50,
                TarjetaGuid = "tarjeta-guid",
                ClienteGuid = "cliente-guid",
                ProductoGuid = "producto-guid",
                CreatedAt = DateTime.UtcNow.ToString("o"),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
                IsDeleted = false
            }
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetAllMeAsync(user.Guid)).ReturnsAsync(expectedCuentas);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetAllMeAccounts();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAllMeAccountsNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetAllMeAsync(user.Guid))
            .ThrowsAsync(new ClienteNotFoundException("Cliente no encontrado"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetAllMeAccounts();

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAllMeAccountsException()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetAllMeAsync(user.Guid))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetAllMeAccounts();

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    [Test]
    public async Task GetAllMeAccountsNotAuthenticated()
    {
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetAllMeAccounts();

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }


    [Test]
    public async Task GetMeByIbanOk()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var iban = "ES9121000418450200051332";
        var expectedCuenta = new CuentaResponse
        {
            Guid = "guid2",
            Iban = "ES9121000418450200051333",
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetMeByIbanAsync(user.Guid, iban)).ReturnsAsync(expectedCuenta);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }
    
    [Test]
    public async Task GetMeByIbanNotAuthenticated()
    {
        var iban = "ES9121000418450200051332";
        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    
    [Test]
    public async Task GetMeByIbanNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var iban = "ES9121000418450200051332";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetMeByIbanAsync(user.Guid, iban)).ReturnsAsync((CuentaResponse)null);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetMeByIbanNotFoundNoPertenece()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var iban = "ES9121000418450200051332";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetMeByIbanAsync(user.Guid, iban))
            .ThrowsAsync(new CuentaNoPertenecienteAlUsuarioException("Cuenta no pertenece al usuario"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetMeByIbanClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var iban = "ES9121000418450200051332";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetMeByIbanAsync(user.Guid, iban))
            .ThrowsAsync(new ClienteNotFoundException("Cliente no encontrado"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetMeByIbanException()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var iban = "ES9121000418450200051332";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.GetMeByIbanAsync(user.Guid, iban))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.GetMeByIban(iban);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task CreateOk()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "producto-guid",
        };

        var expectedCuenta = new CuentaResponse
        {
            Guid = "guid2",
            Iban = "ES9121000418450200051333",
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.CreateAsync(user.Guid, cuentaRequest)).ReturnsAsync(expectedCuenta);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Create(cuentaRequest);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }
    
    [Test]
    public async Task CreateNotAuthenticated()
    {
        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "producto-guid",
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        
        var result = await _controller.Create(cuentaRequest);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }
    
    [Test]
    public async Task CreateProductoNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "producto-guid",
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.CreateAsync(user.Guid, cuentaRequest))
            .ThrowsAsync(new ProductoNotExistException("El producto especificado no existe"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Create(cuentaRequest);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task CreateClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "producto-guid",
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.CreateAsync(user.Guid, cuentaRequest))
            .ThrowsAsync(new ClienteNotFoundException("Cliente no encontrado"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Create(cuentaRequest);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateException()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaRequest = new CuentaRequest
        {
            TipoCuenta = "producto-guid",
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.CreateAsync(user.Guid, cuentaRequest))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Create(cuentaRequest);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    [Test]
    public async Task DeleteOk()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaGuid = "cuenta-guid";
        var expectedCuenta = new CuentaResponse
        {
            Guid = "guid2",
            Iban = "ES9121000418450200051333",
            Saldo = 1500.50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            IsDeleted = false
        };

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.DeleteMeAsync(user.Guid, cuentaGuid)).ReturnsAsync(expectedCuenta);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }
    
    [Test]
    public async Task DeleteNotAuthenticated()
    {
        var cuentaGuid = "cuenta-guid";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns((Banco_VivesBank.User.Models.User)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedObjectResult>());
    }
    
    [Test]
    public async Task DeleteCuentaNotFound()
    {
        
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaGuid = "cuenta-guid";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.DeleteMeAsync(user.Guid, cuentaGuid)).ReturnsAsync((CuentaResponse)null);

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task DeleteClienteNotFound()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaGuid = "cuenta-guid";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.DeleteMeAsync(user.Guid, cuentaGuid))
            .ThrowsAsync(new ClienteNotFoundException("Cliente no encontrado"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task DeleteNoPertenecienteException()
    {
        
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaGuid = "cuenta-guid";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.DeleteMeAsync(user.Guid, cuentaGuid))
            .ThrowsAsync(new CuentaNoPertenecienteAlUsuarioException("No tienes permisos para eliminar esta cuenta."));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var forbiddenResult = result.Result as ObjectResult;
        Assert.That(forbiddenResult.StatusCode, Is.EqualTo(403));
    }
    
    [Test]
    public async Task DeleteException()
    {
        var user = new Banco_VivesBank.User.Models.User
        {
            Id = 1,
            Guid = "user-guid",
            Username = "cliente1",
            Role = Role.User
        };

        var cuentaGuid = "cuenta-guid";

        _mockUserService.Setup(service => service.GetAuthenticatedUser()).Returns(user);
        _mockCuentaService.Setup(service => service.DeleteMeAsync(user.Guid, cuentaGuid))
            .ThrowsAsync(new Exception("Error interno del servidor"));

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, "Cliente") };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = await _controller.Delete(cuentaGuid);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
}