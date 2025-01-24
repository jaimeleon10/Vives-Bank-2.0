using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Base.Storage;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Base.Controllers;

[ApiController]
[Route("api/productosBase")]
public class BaseController : ControllerBase
{
    private readonly ILogger<BaseController> _logger;
    private readonly IBaseService _baseService;
    private readonly IStorageProductos _storageProductos;
    private readonly PaginationLinksUtils _paginationLinksUtils;

    public BaseController(ILogger<BaseController> logger, IBaseService baseService, IStorageProductos storageProductos, PaginationLinksUtils pagination)
    {
        _logger = logger;
        _baseService = baseService;
        _storageProductos = storageProductos;
        _paginationLinksUtils = pagination;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PageResponse<BaseResponse>>>> GetAll(
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
        var pageResult = await _baseService.GetAllPagedAsync(pageRequest);
            
        var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
        var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
        Response.Headers.Add("link", linkHeader);
            
        return Ok(pageResult);

    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<BaseResponse>> GetByGuid(string guid)
    {
        try
        {
            var baseByGuid = await _baseService.GetByGuidAsync(guid);
        
            if (baseByGuid is null) return NotFound($"Producto con guid: {guid} no encontrado");

            return Ok(baseByGuid);
        }
        catch (BaseException e)
        {
            return BadRequest(e.Message);
        }
        
    }
    
    [HttpPost]
    public async Task<ActionResult<BaseResponse>> Create([FromBody] BaseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _baseService.CreateAsync(request));
        }
        catch (BaseException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{guid}")]
    public async Task<ActionResult<BaseResponse>> Update(string guid, [FromBody] BaseUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        
        try
        {
            var baseResponse = await _baseService.UpdateAsync(guid, dto);
            if (baseResponse is null) return NotFound($"No se ha encontrado el producto con guid: {guid}");
            return Ok(baseResponse);
        }
        catch (BaseException e)
        {
            return BadRequest(e.Message);
        }
        

        
    }

    [HttpDelete("{guid}")]
    public async Task<ActionResult<BaseResponse>> DeleteByGuid(string guid)
    {
        var baseByGuid = await _baseService.DeleteAsync(guid);
        if (baseByGuid is null) return NotFound($"No se ha podido eliminar el producto con guid: {guid}");
        return Ok(baseByGuid);
    }
    
    [HttpPost("import")]
    public async Task<ActionResult<IEnumerable<BaseResponse>>> ImportFromCsv(IFormFile file)
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

            var responses = new List<BaseResponse>();

            foreach (var product in importedProducts)
            {
                var request = new BaseRequest
                {
                    Nombre = product.Nombre,
                    Descripcion = product.Descripcion,
                    TipoProducto = product.TipoProducto,
                    Tae = product.Tae
                };

                var response = await _baseService.CreateAsync(request);
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
    public async Task<IActionResult> ExportToCsv()
    {
        try
        {
            var products = await _baseService.GetAllAsync();
            if (!products.Any())
                return Ok("No hay productos para exportar");

            var tempFilePath = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFilePath);

            var productsToExport = products.Select(p => new Models.Base
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