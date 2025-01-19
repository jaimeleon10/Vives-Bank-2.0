using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Base.Controllers;

[ApiController]
[Route("api/productosBase")]
public class BaseController : ControllerBase
{
    private readonly IBaseService _baseService;

    public BaseController(IBaseService baseService)
    {
        _baseService = baseService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaseResponse>>> GetAll()
    {
        return Ok(await _baseService.GetAllAsync());
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<BaseResponse>> GetByGuid(string guid)
    {
        var baseByGuid = await _baseService.GetByGuidAsync(guid);
        
        if (baseByGuid is null) return NotFound($"Producto con guid: {guid} no encontrado");

        return Ok(baseByGuid);
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

        var baseResponse = await _baseService.UpdateAsync(guid, dto);

        if (baseResponse is null) return NotFound($"No se ha podido actualizar el producto con guid: {guid}");
        return Ok(baseResponse);
    }

    [HttpDelete("{guid}")]
    public async Task<ActionResult<BaseResponse>> DeleteByGuid(string guid)
    {
        var baseByGuid = await _baseService.DeleteAsync(guid);
        if (baseByGuid is null) return NotFound($"No se ha podido eliminar el producto con guid: {guid}");
        return Ok(baseByGuid);
    }
}