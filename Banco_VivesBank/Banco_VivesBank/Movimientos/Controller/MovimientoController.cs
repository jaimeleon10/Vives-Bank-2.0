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
[Produces("application/json")] 
[Tags("Movimientos")] 
public class MovimientoController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;
    private readonly IUserService _userService;
    
    public MovimientoController(IMovimientoService movimientoService, IUserService userService)
    {
        _movimientoService = movimientoService;
        _userService = userService;
    }

    /// <summary>
    /// Obtiene todos los movimientos
    /// </summary>
    /// <returns>Devuelve un ActionResult junto con una lista de movimientos</returns>
    /// <response code="200">Devuelve una lista de movimientos</response>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(IEnumerable<MovimientoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetAll()
    {
        return Ok(await _movimientoService.GetAllAsync());
    }

    /// <summary>
    /// Obtiene un movimiento a través de su GUID
    /// </summary>
    /// <param name="guid">GUID del movimiento que se busca</param>
    /// <returns>Devuelve un ActionResult junto con el movimiento buscado</returns>
    /// <response code="200">Devuelve el movimiento</response>
    /// <response code="404">No se han encontrado el movimiento</response>
    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")] 
    [ProducesResponseType(typeof(MovimientoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovimientoResponse>> GetByGuid(string guid)
    {
        var movimiento = await _movimientoService.GetByGuidAsync(guid);
        
        if (movimiento == null) return NotFound(new { message = $"No se ha encontrado el movimiento con guid: {guid}"});
        
        return Ok(movimiento);
    }
    
    /// <summary>
    /// Obtiene los movimientos pertenecientes a un cliente a través de su GUID
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente del cual sacar sus movimeintos</param>
    /// <returns>Devuelve un ActionResult junto con la lista de movimientos del cliente</returns>
    /// <response code="200">Devuelve una lista de movimientos</response>
    [HttpGet("cliente/{clienteGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(IEnumerable<MovimientoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetByClienteGuid(string clienteGuid)
    {
        return Ok(await _movimientoService.GetByClienteGuidAsync(clienteGuid));
    }
    
    /// <summary>
    /// Obtiene los movimiento del cliente autenticado
    /// </summary>
    /// <returns>Devuelve un ActionResult junto con la lista de movimientos del cliente autenticado</returns>
    /// <response code="200">Devuelve una lista de movimientos</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]    
    [ProducesResponseType(typeof(IEnumerable<MovimientoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetMyMovimientos()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        return Ok(await _movimientoService.GetMyMovimientos(userAuth));
    }
    
    /// <summary>
    /// Crea un movimiento de nómina
    /// </summary>
    /// <param name="ingresoNominaRequest">Modelo de solicitud con los datos del ingreso de nómina</param>
    /// <returns>Devuelve un ActionResult junto con el ingreso de nómina</returns>
    /// <response code="200">Devuelve el ingreso de nómina</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    /// <response code="400">La cuenta no pertenece al usuario autenticado</response>
    /// <response code="404">No se ha encontrado la cuenta</response>
    /// <response code="400">Error a la hora de actualizar el saldo de la cuenta</response>
    [HttpPost("ingresoNomina")]
    [Authorize(Policy = "ClientePolicy")]    
    [ProducesResponseType(typeof(IngresoNominaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// Crea un movimiento de pago con tarjeta
    /// </summary>
    /// <param name="pagoConTarjetaRequest">Modelo de solicitud con los datos del pago con tarjeta</param>
    /// <returns>Devuelve un ActionResult junto con el pago con tarjeta</returns>
    /// <response code="200">Devuelve el pago con tarjeta</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    /// <response code="404">No se ha encontrado la tarjeta</response>
    /// <response code="400">Saldo de cuenta insuficiente</response>
    /// <response code="400">La cuenta no pertenece al cliente autenticado</response>
    /// <response code="404">No se ha encontrado la cuenta</response>
    /// <response code="400">Error a la hora de actualizar el saldo de la cuenta</response>
    [HttpPost("pagoConTarjeta")]
    [Authorize(Policy = "ClientePolicy")]    
    [ProducesResponseType(typeof(PagoConTarjetaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// Crea un movimiento de transferencia
    /// </summary>
    /// <param name="transferenciaRequest">Modelo de solicitud con los datos de la transferencia</param>
    /// <returns>Devuelve un ActionResult junto con la transferencia</returns>
    /// <response code="200">Devuelve la transferencia</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    /// <response code="400">Saldo de cuenta insuficiente</response>
    /// <response code="400">La cuenta no pertenece al cliente autenticado</response>
    /// <response code="400">Las cuentas de origen y destino deben ser distintas</response>
    /// <response code="400">Error a la hora de actualizar el saldo de la cuenta</response>
    [HttpPost("transferencia")]
    [Authorize(Policy = "ClientePolicy")]   
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// Crea un movimiento de revocar transferencia
    /// </summary>
    /// <param name="movimientoGuid">GUID del movimiento de transferencia a revocar</param>
    /// <returns>Devuelve un ActionResult junto con la transferencia revocada</returns>
    /// <response code="200">Devuelve la transferencia</response>
    /// <response code="404">No se ha encontrado el movimiento</response>
    /// <response code="400">El movimiento no pertenece al cliente autenticado</response>
    /// <response code="404">La transferencia no es de tipo recibida</response>
    /// <response code="400">Error a la hora de actualizar el saldo de la cuenta</response>
    /// <response code="400">La cuenta no pertenece al cliente autenticado</response>
    [HttpPost("transferencia/revocar/{movimientoGuid}")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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