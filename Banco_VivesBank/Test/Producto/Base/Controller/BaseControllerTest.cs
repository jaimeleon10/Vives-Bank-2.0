using System.Text;
using Banco_VivesBank.Producto.ProductoBase.Controllers;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.ProductoBase.Storage;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Producto.Base.Controller;

public class BaseControllerTests
{
    private Mock<ILogger<ProductoController>> _loggerMock;
    private Mock<IProductoService> _baseServiceMock;
    private Mock<IStorageProductos> _storageProductosMock;
    private ProductoController _controller;
    private Mock<PaginationLinksUtils> _paginationLinksUtils;
    
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ProductoController>>();
        _baseServiceMock = new Mock<IProductoService>();
        _storageProductosMock = new Mock<IStorageProductos>();
        _paginationLinksUtils = new Mock<PaginationLinksUtils>();
        _controller = new ProductoController(
            _loggerMock.Object,
            _baseServiceMock.Object,
            _storageProductosMock.Object,
            _paginationLinksUtils.Object
        );
    }

    [Test]
    public async Task GetAll()
    {
        var expectedProducts = new List<ProductoResponse>
        {
            new() { Nombre = "Producto1", Descripcion = "Descripcion1", Tae = 5.5, TipoProducto = "Tipo1" },
            new() { Nombre = "Producto2", Descripcion = "Descripcion2", Tae = 6.5, TipoProducto = "Tipo2" }
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

        var pageResponse = new PageResponse<ProductoResponse>
        {
            Content = expectedProducts,
            TotalElements = expectedProducts.Count,
            PageNumber = pageRequest.PageNumber,
            PageSize = pageRequest.PageSize,
            TotalPages = 1
        };

        _baseServiceMock.Setup(s => s.GetAllPagedAsync(pageRequest))
            .ReturnsAsync(pageResponse);

        var baseUri = new Uri("http://localhost/api/productosBase");
        _paginationLinksUtils.Setup(utils => utils.CreateLinkHeader(pageResponse, baseUri))
            .Returns("<http://localhost/api/productosBase?page=0&size=5>; rel=\"prev\",<http://localhost/api/productosBase?page=2&size=5>; rel=\"next\"");

        // Configurar el contexto HTTP para la prueba
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost"),
                PathBase = new PathString("/api/productosBase")
            }
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var result = await _controller.GetAll(page, size, sortBy, direction);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task GetByGuid()
    {
        var guid = "testGuid";
        var expectedProduct = new ProductoResponse
        {
            Nombre = "Producto1",
            Descripcion = "Descripcion1",
            Tae = 5.5,
            TipoProducto = "Tipo1"
        };

        _baseServiceMock.Setup(s => s.GetByGuidAsync(guid))
            .ReturnsAsync(expectedProduct);

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (result.Result as OkObjectResult)?.Value as ProductoResponse;
        Assert.That(returnValue, Is.EqualTo(expectedProduct));

    }

    [Test]
    public async Task GetByGuidNotFound()
    {
        var guid = "non-existing-guid";
        _baseServiceMock.Setup(s => s.GetByGuidAsync(guid))
            .ReturnsAsync((ProductoResponse)null);

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetByGuidExceptionNotFound()
    {
        var guid = "testGuid";

        _baseServiceMock.Setup(s => s.GetByGuidAsync(guid))
            .ThrowsAsync(new ProductoException("Error message"));

        var result = await _controller.GetByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create()
    {
        var request = new ProductoRequest()
        {
            Nombre = "nuevo producto",
            Descripcion = "nueva descripcion",
            Tae = 5.5,
            TipoProducto = "nuevo tipo"
        };

        var expectedResponse = new ProductoResponse
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Tae = request.Tae,
            TipoProducto = request.TipoProducto
        };

        _baseServiceMock.Setup(s => s.CreateAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Create(request);
        
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var returnValue = (result.Result as OkObjectResult)?.Value as ProductoResponse;
        Assert.That(returnValue, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task CreateBaseRequestInvalido()
    {
        var request = new ProductoRequest();
        _controller.ModelState.AddModelError("Nombre", "Required");

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateBadRequest()
    {
        var request = new ProductoRequest
        {
            Nombre = "nuevo producto",
            Descripcion = "nueva Descripcion",
            Tae = 5.5,
            TipoProducto = "nuevo tipo"
        };

        _baseServiceMock.Setup(s => s.CreateAsync(request))
            .ThrowsAsync(new ProductoException("Error message"));

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update()
    {
        var guid = "testGuid";
        var updateDto = new ProductoRequestUpdate
        {
            Nombre = "Producto actualizado",
            Descripcion = "Descripcion actualizada",
            Tae = 6.5
        };

        var expectedResponse = new ProductoResponse
        {
            Nombre = updateDto.Nombre,
            Descripcion = updateDto.Descripcion,
            Tae = updateDto.Tae
        };

        _baseServiceMock.Setup(s => s.UpdateAsync(guid, updateDto))
                        .ReturnsAsync(expectedResponse);

        var result = await _controller.Update(guid, updateDto);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnValue = okResult?.Value as ProductoResponse;
        Assert.That(returnValue, Is.EqualTo(expectedResponse));
    }
    
    [Test]
    public async Task UpdateBadRequest()
    {
        var guid = "valid-guid";
        _controller.ModelState.AddModelError("Nombre", "El campo es requerido");
        var baseRequest = new ProductoRequestUpdate();

        var result = await _controller.Update(guid, baseRequest);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task UpdateBadRequestException()
    {
        var guid = "testGuid";
        var request = new ProductoRequestUpdate()
        {
            Nombre = "nuevo producto",
            Descripcion = "nueva Descripcion",
            Tae = 5.5
        };

        _baseServiceMock.Setup(s => s.UpdateAsync(guid, request))
            .ThrowsAsync(new ProductoException("Error message"));

        var result = await _controller.Update(guid, request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task UpdateNotFound()
    {
        var guid = "nonexistent-guid";
        var baseRequest = new ProductoRequestUpdate
        {
            Nombre = "NuevoNombre",
            Descripcion = "NuevaDescripcion",
            Tae = 6.5
        };

        _baseServiceMock.Setup(service => service.UpdateAsync(guid, baseRequest))
            .ReturnsAsync((ProductoResponse)null);

        var result = await _controller.Update(guid, baseRequest);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo($"No se ha encontrado el producto con guid: {guid}"));
    }

    [Test]
    public async Task Delete()
    {
        var guid = "testGuid";
        var expectedResponse = new ProductoResponse
        {
            Nombre = "Producto",
            Descripcion = "Descripcion",
            Tae = 5.5,
            TipoProducto = "Tipo"
        };

        _baseServiceMock.Setup(s => s.DeleteByGuidAsync(guid))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.DeleteByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var returnValue = (result.Result as OkObjectResult)?.Value as ProductoResponse;
        Assert.That(returnValue, Is.EqualTo(expectedResponse));
    }
    
    [Test]
    public async Task DeleteNotFound()
    {
        var guid = "nonexistent-guid";

        _baseServiceMock.Setup(service => service.DeleteByGuidAsync(guid))
            .ReturnsAsync((ProductoResponse)null);

        var result = await _controller.DeleteByGuid(guid);

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo("No se ha podido eliminar el producto con guid: nonexistent-guid"));
    }

    [Test]
    public async Task ImportFromCsv()
    {
        var fileContent = "Nombre,Descripcion,TipoProducto,Tae\nProducto1,Desc1,Tipo1,5.5";
        var fileName = "test.csv";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(stream.Length);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((stream, token) =>
            {
                stream.Write(Encoding.UTF8.GetBytes(fileContent), 0, fileContent.Length);
            })
            .Returns(Task.CompletedTask);

        var importedProducts = new List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>
        {
            new() { Nombre = "Producto1", Descripcion = "Desc1", TipoProducto = "Tipo1", Tae = 5.5 }
        };

        _storageProductosMock.Setup(s => s.ImportProductosFromCsv(It.IsAny<FileInfo>()))
            .Returns(importedProducts);

        var expectedResponse = new ProductoResponse
        {
            Nombre = "Producto1",
            Descripcion = "Desc1",
            TipoProducto = "Tipo1",
            Tae = 5.5
        };

        _baseServiceMock.Setup(s => s.CreateAsync(It.IsAny<ProductoRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.ImportFromCsv(formFile.Object);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okObjectResult = result.Result as OkObjectResult;
        var returnValue = okObjectResult?.Value as List<ProductoResponse>;
        Assert.That(returnValue?.Count, Is.EqualTo(1));
        Assert.That(returnValue?[0], Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task ExportToCsv()
    {
        var products = new List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>
        {
            new() { Nombre = "Producto1", Descripcion = "Desc1", TipoProducto = "Tipo1", Tae = 5.5 }
        };

        _baseServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(products);

        var result = await _controller.ExportToCsv();

        Assert.That(result, Is.TypeOf<FileContentResult>());
        var fileResult = result as FileContentResult;
        Assert.That(fileResult.ContentType, Is.EqualTo("text/csv"));
        Assert.That(fileResult.FileDownloadName, Does.StartWith("productos_export_"));
        Assert.That(fileResult.FileDownloadName, Does.EndWith(".csv"));
    }
    
    [Test]
    public async Task ImportFromCsvArchivoNull()
    {
        IFormFile file = null;
        var result = await _controller.ImportFromCsv(file);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("No se ha proporcionado ningún archivo"));
    }
    
    [Test]
    public async Task ImportFromCsvSinCsv()
    {
        var fileContent = "Nombre,Descripcion,TipoProducto,Tae\nProducto1,Desc1,Tipo1,5.5";
        var fileName = "test.txt";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(stream.Length);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ImportFromCsv(formFile.Object);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("El archivo debe ser un CSV"));
    }

    [Test]
    public async Task ImportFromCsvException()
    {
        var fileContent = "Nombre,Descripcion,TipoProducto,Tae\nProducto1,Desc1,Tipo1,5.5";
        var fileName = "test.csv";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(stream.Length);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storageProductosMock.Setup(s => s.ImportProductosFromCsv(It.IsAny<FileInfo>()))
            .Returns(new List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>());

        var result = await _controller.ImportFromCsv(formFile.Object);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("No se pudieron importar los productos del archivo CSV"));
    }

    [Test]
    public async Task ImportFromCsvErrInterno()
    {
        var fileContent = "Nombre,Descripcion,TipoProducto,Tae\nProducto1,Desc1,Tipo1,5.5";
        var fileName = "test.csv";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(stream.Length);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storageProductosMock.Setup(s => s.ImportProductosFromCsv(It.IsAny<FileInfo>()))
            .Throws(new Exception("Error inesperado"));

        var result = await _controller.ImportFromCsv(formFile.Object);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error al procesar el archivo: Error inesperado"));
    }

    [Test]
    public async Task ExportToCsvSinProductos()
    {
        _baseServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(new List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>());

        var result = await _controller.ExportToCsv();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo("No hay productos para exportar"));
    }

    [Test]
    public async Task ExportToCsvErrInterno()
    {
        var products = new List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>
        {
            new() { Nombre = "Producto1", Descripcion = "Desc1", TipoProducto = "Tipo1", Tae = 5.5 }
        };

        _baseServiceMock.Setup(s => s.GetAllForStorage())
            .ReturnsAsync(products);

        _storageProductosMock.Setup(s => s.ExportProductosFromCsv(It.IsAny<FileInfo>(),
                It.IsAny<List<Banco_VivesBank.Producto.ProductoBase.Models.Producto>>()))
            .Throws(new Exception("Error al exportar"));

        var result = await _controller.ExportToCsv();

        Assert.That(result, Is.TypeOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error al exportar los productos: Error al exportar"));
    }
}