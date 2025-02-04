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
    
    /// <summary>
    /// Obtiene todos los productos paginados y ordenados según los parámetros especificados.
    /// </summary>
    /// <param name="page">Número de la página a la que se quiere acceder</param>
    /// <param name="size">Número de productos por página</param>
    /// <param name="sortBy">Parámetro por el que se ordenan los productos</param>
    /// <param name="direction">Dirección de ordenación, ascendente (asc) o descendente (desc)</param>
    /// <returns>Devuelve un ActionResult con una lista paginada de productos</returns>
    /// <response code="200">Devuelve una lista de productos paginados</response>
    /// <response code="400">Ocurrió un error en la solicitud</response>
    [HttpGet]
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

    /// <summary>
    /// Obtiene un producto específico por su identificador único (GUID).
    /// </summary>
    /// <param name="guid">Identificador único del producto</param>
    /// <returns>Devuelve un ActionResult con los detalles del producto</returns>
    /// <response code="200">Devuelve el producto encontrado</response>
    /// <response code="404">No se encontró un producto con el GUID proporcionado</response>
    [HttpGet("{guid}")]
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
    
    /// <summary>
    /// Crea un nuevo producto en el sistema.
    /// </summary>
    /// <param name="request">Objeto con los datos necesarios para crear el producto.</param>
    /// <returns>Devuelve un ActionResult con el producto creado o un mensaje de error en caso de fallo.</returns>
    /// <response code="200">Devuelve el producto creado con éxito.</response>
    /// <response code="400">En caso de error de validación o excepciones específicas del producto.</response>
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

    /// <summary>
    /// Actualiza un producto existente en el sistema.
    /// </summary>
    /// <param name="guid">Identificador único del producto que se quiere actualizar.</param>
    /// <param name="request">Objeto con los datos actualizados del producto.</param>
    /// <returns>Devuelve un ActionResult con el producto actualizado o un mensaje de error si no se encuentra el producto.</returns>
    /// <response code="200">Devuelve el producto actualizado con éxito.</response>
    /// <response code="400">En caso de error de validación o de producto específico.</response>
    /// <response code="404">Cuando no se encuentra el producto con el identificador especificado.</response>
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

    /// <summary>
    /// Elimina un producto en el sistema mediante su identificador único.
    /// </summary>
    /// <param name="guid">Identificador único del producto a eliminar.</param>
    /// <returns>Devuelve un ActionResult con el producto eliminado o un mensaje de error si no se encuentra el producto.</returns>
    /// <response code="200">Devuelve el producto eliminado con éxito.</response>
    /// <response code="404">Cuando no se encuentra el producto con el identificador especificado.</response>
    [HttpDelete("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponse>> DeleteByGuid(string guid)
    {
        var baseByGuid = await _productoService.DeleteByGuidAsync(guid);
        if (baseByGuid is null) return NotFound($"No se ha podido eliminar el producto con guid: {guid}");
        return Ok(baseByGuid);
    }
    
    /// <summary>
    /// Importa productos desde un archivo CSV al sistema.
    /// </summary>
    /// <param name="file">Archivo CSV que contiene los datos de los productos a importar.</param>
    /// <returns>Devuelve un ActionResult con la lista de productos creados o un mensaje de error en caso de fallo.</returns>
    /// <response code="200">Devuelve la lista de productos creados con éxito.</response>
    /// <response code="400">En caso de errores con el archivo CSV o si no se pueden importar productos.</response>
    /// <response code="500">Cuando ocurre un error interno durante el procesamiento del archivo.</response>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
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

                try
                {
                    var response = await _productoService.CreateAsync(request);
                    responses.Add(response);
                }
                catch (ProductoExistByNameException ex)
                {
                    _logger.LogWarning($"El producto '{product.Nombre}' ya existe, omitiendo...");
                }
                catch (ProductoDuplicatedException ex)
                {
                    _logger.LogWarning($"El producto con tipo '{product.TipoProducto}' ya existe, omitiendo...");
                }
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
    /// <summary>
    /// Exporta todos los productos del sistema a un archivo CSV.
    /// </summary>
    /// <returns>Devuelve un archivo CSV con los productos exportados o un mensaje en caso de que no haya productos para exportar.</returns>
    /// <response code="200">Devuelve el archivo CSV con los productos exportados.</response>
    /// <response code="500">Cuando ocurre un error interno al exportar los productos.</response>
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
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