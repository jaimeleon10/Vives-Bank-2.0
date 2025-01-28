using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Movimientos.Controller;

[ApiController]
[Route("api/domiciliaciones")]
public class DomiciliacionController : ControllerBase
{
    private readonly IDomiciliacionService _domiciliacionService;

    public DomiciliacionController(IDomiciliacionService domiciliacionService)
    {
        _domiciliacionService = domiciliacionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetAllDomiciliaciones()
    {
        return Ok(await _domiciliacionService.GetAllAsync());
    }
    
    [HttpGet("{domiciliacionGuid}")]
    public async Task<ActionResult<DomiciliacionResponse>> GetDomiciliacionByGuid(string domiciliacionGuid)
    {
        var domiciliacion = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        if (domiciliacion == null) return NotFound($"No se ha encontrado la domiciliación con guid: {domiciliacionGuid}");
        
        return Ok(domiciliacion);
    }
    
    [HttpGet("cliente/{clienteGuid}")]
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetDomiciliacionesByClienteGuid(string clienteGuid)
    {
        return Ok(await _domiciliacionService.GetByClienteGuidAsync(clienteGuid));
    }

    [HttpPost]
    public async Task<ActionResult<DomiciliacionResponse>> CreateDomiciliacion([FromBody] DomiciliacionRequest domiciliacionRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _domiciliacionService.CreateAsync(domiciliacionRequest));
        }
        catch (ClienteException e)
        {
            return NotFound(e.Message);
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{domiciliacionGuid}")]
    public async Task<ActionResult<DomiciliacionResponse?>> DesactivateDomiciliacion(string domiciliacionGuid)
    {
        try
        {
            var domiciliacion = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
            if (domiciliacion != null) return Ok(domiciliacion);
            return BadRequest($"No se ha encontrado domiciliacion con guid {domiciliacionGuid}");
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
}