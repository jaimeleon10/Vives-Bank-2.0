using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.ProductoBase.Storage;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

namespace Banco_VivesBank.Producto.ProductoBase.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductoController : ControllerBase
{
    private readonly ILogger<ProductoController> _logger;
    private readonly IProductoService _productoService;
    private readonly IStorageProductos _storageProductos;
    private readonly PaginationLinksUtils _paginationLinksUtils;

    public ProductoController(ILogger<ProductoController> logger, IProductoService productoService, IStorageProductos storageProductos, PaginationLinksUtils pagination)
    {
        _logger = logger;
        _productoService = productoService;
        _storageProductos = storageProductos;
        _paginationLinksUtils = pagination;
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PageResponse<ProductoResponse>>>> GetAll(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "id",
        [FromQuery] string direction = "asc")
    {
        
        var pageRequest = new PageRequest
        {
            PageNumber = page,
            PageSize = size,
            SortBy = sortBy,
            Direction = direction
        };
        var pageResult = await _productoService.GetAllPagedAsync(pageRequest);
            
        var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
        var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
        Response.Headers.Append("link", linkHeader);
            
        return Ok(pageResult);
    }

    [HttpGet("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponse>> GetByGuid(string guid)
    {
        try
        {
            var baseByGuid = await _productoService.GetByGuidAsync(guid);
        
            if (baseByGuid is null) return NotFound($"Producto con guid: {guid} no encontrado");

            return Ok(baseByGuid);
        }
        catch (ProductoException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponse>> Create([FromBody] ProductoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _productoService.CreateAsync(request));
        }
        catch (ProductoException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponse>> Update(string guid, [FromBody] ProductoRequestUpdate request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var baseResponse = await _productoService.UpdateAsync(guid, request);
            if (baseResponse is null) return NotFound($"No se ha encontrado el producto con guid: {guid}");
            return Ok(baseResponse);
        }
        catch (ProductoException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponse>> DeleteByGuid(string guid)
    {
        var baseByGuid = await _productoService.DeleteByGuidAsync(guid);
        if (baseByGuid is null) return NotFound($"No se ha podido eliminar el producto con guid: {guid}");
        return Ok(baseByGuid);
    }
    
    [HttpPost("import")]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> ImportFromCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se ha proporcionado ningún archivo");

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("El archivo debe ser un CSV");

        try
        {
            var tempFilePath = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFilePath);

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var importedProducts = _storageProductos.ImportProductosFromCsv(fileInfo);

            if (!importedProducts.Any())
                return BadRequest("No se pudieron importar los productos del archivo CSV");

            var responses = new List<ProductoResponse>();

            foreach (var product in importedProducts)
            {
                var request = new ProductoRequest
                {
                    Nombre = product.Nombre,
                    Descripcion = product.Descripcion,
                    TipoProducto = product.TipoProducto,
                    Tae = product.Tae,
                    IsDeleted = false
                };

                var response = await _productoService.CreateAsync(request);
                responses.Add(response);
            }

            if (fileInfo.Exists)
            {
                fileInfo.Delete(); 
            }

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
        }
    }

    [HttpGet("export")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportToCsv()
    {
        try
        {
            var products = await _productoService.GetAllForStorage();
            if (!products.Any())
                return Ok("No hay productos para exportar");

            var tempFilePath = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFilePath);

            var productsToExport = products.Select(p => new ProductoBase.Models.Producto
            {
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                TipoProducto = p.TipoProducto,
                Tae = p.Tae
            }).ToList();

            _storageProductos.ExportProductosFromCsv(fileInfo, productsToExport);

            var fileBytes = System.IO.File.ReadAllBytes(tempFilePath);

            try
            {
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }
            catch (IOException deleteEx)
            {
                _logger.LogError($"Error al eliminar el archivo temporal: {deleteEx.Message}");
            }

            return File(fileBytes, "text/csv", $"productos_export_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al exportar los productos: {ex.Message}");
        }
    }
}