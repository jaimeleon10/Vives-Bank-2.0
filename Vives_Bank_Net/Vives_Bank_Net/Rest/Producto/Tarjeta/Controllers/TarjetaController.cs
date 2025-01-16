using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Dto;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Models;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Services;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Controllers;


[ApiController]
[Route ("api/vives-bank/tarjeta")]
public class TarjetaController : ControllerBase
{
    private readonly ITarjetaService _tarjetaService;

    public TarjetaController(ITarjetaService tarjetaService)
    {
        _tarjetaService = tarjetaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.Tarjeta>>> GetAllTarjetas()
    {
        var tarjetas = await _tarjetaService.GetAllAsync();
        return Ok(tarjetas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TarjetaResponseDto>> GetTarjetaById(string id)
    {
        var tarjeta = await _tarjetaService.GetByGuidAsync(id);
        if (tarjeta == null) return NotFound();
        return Ok(tarjeta);
    }

    [HttpPost]
    public async Task<ActionResult<Models.Tarjeta>> CreateTarjeta([FromBody] TarjetaRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tarjetaModel = await _tarjetaService.CreateAsync(dto);
            
            return CreatedAtAction(nameof(GetTarjetaById), new {id = tarjetaModel.Id}, tarjetaModel);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TarjetaResponseDto>> UpdateTarjeta(string id, [FromBody] TarjetaRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tarjeta = await _tarjetaService.GetByGuidAsync(id);
        if (tarjeta == null) return NotFound();

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
        if (tarjeta == null) return NotFound();

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