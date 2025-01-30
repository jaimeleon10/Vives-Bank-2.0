using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Movimientos.Controller;

[ApiController]
[Route("api/domiciliaciones")]
public class DomiciliacionController : ControllerBase
{
    private readonly IDomiciliacionService _domiciliacionService;
    private readonly IUserService _userService;

    public DomiciliacionController(IDomiciliacionService domiciliacionService, IUserService userService)
    {
        _domiciliacionService = domiciliacionService;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetAllDomiciliaciones()
    {
        return Ok(await _domiciliacionService.GetAllAsync());
    }
    
    [HttpGet("{domiciliacionGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<DomiciliacionResponse>> GetDomiciliacionByGuid(string domiciliacionGuid)
    {
        try
        {
            var domiciliacion = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
            if (domiciliacion == null) return NotFound(new { message = $"No se ha encontrado la domiciliación con guid: {domiciliacionGuid}"});
            return Ok(domiciliacion);
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }
    
    [HttpGet("cliente/{clienteGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetDomiciliacionesByClienteGuid(string clienteGuid)
    {
        return Ok(await _domiciliacionService.GetByClienteGuidAsync(clienteGuid));
    }
    
    [HttpGet("cliente")]
    [Authorize(Policy = "ClientePolicy")]    
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetMyDomiciliaciones()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        return Ok(await _domiciliacionService.GetMyDomiciliaciones(userAuth));
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
            return NotFound(new { message = e.Message});
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (CuentaException e)
        {
            return NotFound(new { message = e.Message});
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }

    [HttpDelete("{domiciliacionGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<DomiciliacionResponse?>> DesactivateDomiciliacion(string domiciliacionGuid)
    {
        try
        {
            var domiciliacion = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
            if (domiciliacion != null) return Ok(domiciliacion);
            return BadRequest(new { message = $"No se ha encontrado domiciliacion con guid {domiciliacionGuid}"});
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }
}