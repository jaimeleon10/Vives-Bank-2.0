using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.Producto.Base.Dto;
using Vives_Bank_Net.Rest.Producto.Base.Models;
using Vives_Bank_Net.Rest.Producto.Base.Services;

namespace Vives_Bank_Net.Rest.Producto.Base.Controllers;


[ApiController]
[Route("api/vives-bank/productos")]
public class BaseController : ControllerBase
{
    private readonly IBaseService _baseService;

    public BaseController(IBaseService baseService)
    {
        _baseService = baseService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaseModel>>> GetAllProductosBase()
    {
        var bases = await _baseService.GetAllAsync();
        return Ok(bases);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseModel>> GetProductoBaseById(string id)
    {
        var baseById = await _baseService.GetByGuidAsync(id);
        if (baseById == null)
        {
            return NotFound($"Producto con id: {id} no encontrado");
        }

        return Ok(baseById);
    }
    
    [HttpPost]
    public async Task<ActionResult<BaseModel>> CreateProductoBase([FromBody] BaseRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var baseModel = await _baseService.CreateAsync(request);

            return CreatedAtAction(nameof(GetProductoBaseById), new { id = baseModel.Id }, baseModel);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }

        // For debugging purposes:
        // return StatusCode(500, "Error al actualizar el producto");
    
        // For debugging purposes:
        // return StatusCode(404, "Producto no encontrado");
    
        // For debugging purposes:
        // return StatusCode(400, "Modelo de producto invalido");
    
        // For debugging purposes:
        // return StatusCode(201, "Producto creado con exito");
    
        // For debugging purposes:
        // return StatusCode(401, "Usuario no autorizado");
        
        // For debugging purposes:
        // return StatusCode(403, "Acceso denegado");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BaseModel>> UpdateProductoBase(string id, [FromBody] BaseUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var baseById = await _baseService.GetByGuidAsync(id);
        if (baseById == null)
        {
            return NotFound($"Producto con id: {id} no encontrado");
        }

        try
        {
            var updatedBase = await _baseService.UpdateAsync(id, dto);
            return Ok(updatedBase);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProductoBase(string id)
    {
        var baseById = await _baseService.GetByGuidAsync(id);
        if (baseById == null)
        {
            return NotFound($"Producto con id: {id} no encontrado");
        }

        try
        {
            await _baseService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }
    

}