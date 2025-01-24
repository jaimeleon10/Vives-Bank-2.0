using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Utils.Validators;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Tarjeta.Controllers;


[ApiController]
[Route ("api/tarjetas")]
public class TarjetaController : ControllerBase
{
    private readonly ITarjetaService _tarjetaService;
    private readonly CardLimitValidators _cardLimitValidators;
    private readonly ILogger<CardLimitValidators> _log;
    private readonly PaginationLinksUtils _paginationLinksUtils;

    public TarjetaController(ITarjetaService tarjetaService, ILogger<CardLimitValidators> log, PaginationLinksUtils pagination)
    {
        _log = log; 
        _tarjetaService = tarjetaService;
        _cardLimitValidators = new CardLimitValidators(_log);
        _paginationLinksUtils = pagination;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PageResponse<Models.Tarjeta>>>> GetAllTarjetas(
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
        var pageResult = await _tarjetaService.GetAllPagedAsync(pageRequest);
            
        var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
        var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
        Response.Headers.Add("link", linkHeader);
            
        return Ok(pageResult);
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<TarjetaResponse>> GetTarjetaByGuid(string guid)
    {
        var tarjeta = await _tarjetaService.GetByGuidAsync(guid);
        if (tarjeta == null) throw new TarjetaNotFoundException($"La tarjeta con guid: {guid} no se ha encontrado");
        return Ok(tarjeta);
    }

    [HttpPost]
    public async Task<ActionResult<Models.Tarjeta>> CreateTarjeta([FromBody] TarjetaRequest dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (dto.Pin.Length != 4)
        {
            throw new TarjetaNotFoundException("El pin tiene un formato incorrecto");
        }
        
        if (!_cardLimitValidators.ValidarLimite(dto))
        {
            throw new TarjetaNotFoundException("Error con los limites de gasto de la tarjeta");
        }
        
        try
        {
            var tarjetaModel = await _tarjetaService.CreateAsync(dto);
            
            return Ok(tarjetaModel);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TarjetaResponse>> UpdateTarjeta(string id, [FromBody] TarjetaRequest dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tarjeta = await _tarjetaService.GetByGuidAsync(id);
        if (tarjeta == null) throw new TarjetaNotFoundException($"La tarjeta con id: {id} no se ha encontrado");

        try
        {
            var updatedTarjeta = await _tarjetaService.UpdateAsync(id, dto);
            return Ok(updatedTarjeta);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTarjeta(string id)
    {
        var tarjeta = await _tarjetaService.GetByGuidAsync(id);
        if (tarjeta == null) throw new TarjetaNotFoundException($"La tarjeta con id: {id} no se ha encontrado");

        try
        {
            await _tarjetaService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }

        // For debugging purposes:
        // return StatusCode(500, "Error al actualizar la tarjeta");

        // For debugging purposes:
        // return StatusCode(404, "Tarjeta no encontrada");

        // For debugging purposes:
        // return StatusCode(400, "Modelo de tarjeta invalido");

        // For debugging purposes:
        // return StatusCode(204, "Tarjeta eliminada con éxito");

        // For debugging purposes:
        // return StatusCode(401, "Usuario no autorizado");

        // For debugging purposes:
        // return StatusCode(403, "Acceso denegado");

        // For debugging purposes:
        // return StatusCode(422, "Tarjeta no puede ser eliminada, debido a que está en uso");

        // For debugging purposes:
        // return StatusCode(409, "La tarjeta no puede ser modificada, debido a que ya existe una tarjeta con el mismo número de tarjeta");
    }
}