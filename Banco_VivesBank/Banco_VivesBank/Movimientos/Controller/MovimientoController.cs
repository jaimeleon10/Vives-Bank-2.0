using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CuentaInvalidaException = Banco_VivesBank.Storage.Pdf.Exception;

namespace Banco_VivesBank.Movimientos.Controller;

[ApiController]
[Route("api/movimientos")]
public class MovimientoController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;
    private readonly IUserService _userService;
    
    public MovimientoController(IMovimientoService movimientoService, IUserService userService)
    {
        _movimientoService = movimientoService;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetAll()
    {
        return Ok(await _movimientoService.GetAllAsync());
    }

    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<MovimientoResponse>> GetByGuid(string guid)
    {
        var movimiento = await _movimientoService.GetByGuidAsync(guid);
        
        if (movimiento == null) return NotFound(new { message = $"No se ha encontrado el movimiento con guid: {guid}"});
        
        return Ok(movimiento);
    }
    
    [HttpGet("cliente/{clienteGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetByClienteGuid(string clienteGuid)
    {
        return Ok(await _movimientoService.GetByClienteGuidAsync(clienteGuid));
    }
    
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]    
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetMyMovimientos()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        return Ok(await _movimientoService.GetMyMovimientos(userAuth));
    }
    
    [HttpPost("ingresoNomina")]
    [Authorize(Policy = "ClientePolicy")]    
    public async Task<ActionResult<IngresoNominaResponse>> CreateIngresoNomina([FromBody] IngresoNominaRequest ingresoNominaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _movimientoService.CreateIngresoNominaAsync(userAuth, ingresoNominaRequest));
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
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
    
    [HttpPost("pagoConTarjeta")]
    [Authorize(Policy = "ClientePolicy")]    
    public async Task<ActionResult<PagoConTarjetaResponse>> CreatePagoConTarjeta([FromBody] PagoConTarjetaRequest pagoConTarjetaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest));
        }
        catch (TarjetaException e)
        {
            return NotFound(new { message = e.Message});
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
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
    
    [HttpPost("transferencia")]
    [Authorize(Policy = "ClientePolicy")]    
    public async Task<ActionResult<TransferenciaResponse>> CreateTransferencia([FromBody] TransferenciaRequest transferenciaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _movimientoService.CreateTransferenciaAsync(userAuth, transferenciaRequest));
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (Producto.Cuenta.Exceptions.CuentaInvalidaException e)
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
    
    [HttpPost("transferencia/revocar/{movimientoGuid}")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<TransferenciaResponse>> RevocarTransferencia(string movimientoGuid)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _movimientoService.RevocarTransferenciaAsync(userAuth, movimientoGuid));
        }
        catch (MovimientoNotFoundException e)
        {
            return NotFound(new { message = e.Message});
        }
        catch (MovimientoNoPertenecienteAlUsuarioAutenticadoException e)
        {
            return NotFound(new { message = e.Message});
        }
        catch (TransferenciaEmitidaException e)
        {
            return NotFound(new { message = e.Message});
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (CuentaException e)
        {
            return NotFound(new { message = e.Message});
        }
    }
}