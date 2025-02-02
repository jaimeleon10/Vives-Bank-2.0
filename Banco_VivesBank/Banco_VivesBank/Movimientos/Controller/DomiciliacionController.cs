using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Movimientos.Controller;

[ApiController]
[Route("api/domiciliaciones")]
[Produces("application/json")] 
[Tags("Domiciliaciones")] 
public class DomiciliacionController : ControllerBase
{
    private readonly IDomiciliacionService _domiciliacionService;
    private readonly IUserService _userService;

    public DomiciliacionController(IDomiciliacionService domiciliacionService, IUserService userService)
    {
        _domiciliacionService = domiciliacionService;
        _userService = userService;
    }

    /// <summary>
    /// Obtiene todas las domiciliaciones
    /// </summary>
    /// <returns>Devuelve un ActionResult junto con una lista de domiciliaciones</returns>
    /// <response code="200">Devuelve una lista de domiciliaciones</response>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(IEnumerable<DomiciliacionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetAllDomiciliaciones()
    {
        return Ok(await _domiciliacionService.GetAllAsync());
    }
    
    /// <summary>
    /// Obtiene una domiciliación a través de su GUID
    /// </summary>
    /// <param name="domiciliacionGuid">GUID de la domiciliación que se busca</param>
    /// <returns>Devuelve un ActionResult junto con la domiciliación buscada</returns>
    /// <response code="200">Devuelve la domiciliación</response>
    /// <response code="404">No se han encontrado la domiciliación</response>
    [HttpGet("{domiciliacionGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(DomiciliacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// Obtiene las domiciliaciones pertenecientes a un cliente a través de su GUID
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente del cual sacar sus domiciliaciones</param>
    /// <returns>Devuelve un ActionResult junto con la lista de domiciliaciones del cliente</returns>
    /// <response code="200">Devuelve una lista de domiciliaciones</response>
    [HttpGet("cliente/{clienteGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(IEnumerable<DomiciliacionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetDomiciliacionesByClienteGuid(string clienteGuid)
    {
        return Ok(await _domiciliacionService.GetByClienteGuidAsync(clienteGuid));
    }
    
    /// <summary>
    /// Obtiene los domiciliaciones del cliente autenticado
    /// </summary>
    /// <returns>Devuelve un ActionResult junto con la lista de domiciliaciones del cliente autenticado</returns>
    /// <response code="200">Devuelve una lista de domiciliaciones</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]    
    [ProducesResponseType(typeof(IEnumerable<DomiciliacionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DomiciliacionResponse>>> GetMyDomiciliaciones()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        return Ok(await _domiciliacionService.GetMyDomiciliaciones(userAuth));
    }

    /// <summary>
    /// Crea un movimiento de domiciliación
    /// </summary>
    /// <param name="domiciliacionRequest">Modelo de solicitud con los datos de la domiciliación</param>
    /// <returns>Devuelve un ActionResult junto con la domiciliación</returns>
    /// <response code="200">Devuelve la domiciliación</response>
    /// <response code="404">No se ha podido identificar el cliente autenticado</response>
    /// <response code="404">Cliente no encontrado</response>
    /// <response code="404">Saldo de la cuenta insuficiente</response>
    /// <response code="400">Error a la hora de actualizar el saldo de la cuenta</response>
    [HttpPost]
    [Authorize(Policy = "ClientePolicy")]  
    [ProducesResponseType(typeof(DomiciliacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DomiciliacionResponse>> CreateDomiciliacion([FromBody] DomiciliacionRequest domiciliacionRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
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

    /// <summary>
    /// Desactiva una domiciliación
    /// </summary>
    /// <param name="domiciliacionGuid">GUID de la domiciliación</param>
    /// <returns>Devuelve un ActionResult junto con la domiciliación</returns>
    /// <response code="200">Devuelve la domiciliación</response>
    /// <response code="404">No se ha encontrado la domiciliación</response>
    /// <response code="400">Error a la hora de actualizar la cuenta</response>
    [HttpDelete("{domiciliacionGuid}")]
    [Authorize(Policy = "AdminPolicy")]    
    [ProducesResponseType(typeof(DomiciliacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DomiciliacionResponse?>> DesactivateDomiciliacion(string domiciliacionGuid)
    {
        try
        {
            var domiciliacion = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
            if (domiciliacion != null) return Ok(domiciliacion);
            return NotFound(new { message = $"No se ha encontrado domiciliacion con guid {domiciliacionGuid}"});
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }
    
    /// <summary>
    /// Desactiva una domiciliación propia
    /// </summary>
    /// <param name="domiciliacionGuid">GUID de la domiciliación</param>
    /// <returns>Devuelve un ActionResult junto con la domiciliación</returns>
    /// <response code="200">Devuelve la domiciliación</response>
    /// <response code="400">Domiciliacion no perteneciente al cliente autenticado</response>
    /// <response code="404">No se ha encontrado la domiciliación</response>
    /// <response code="400">Error a la hora de actualizar la cuenta</response>
    [HttpDelete("me/{domiciliacionGuid}")]
    [Authorize(Policy = "ClientePolicy")]    
    [ProducesResponseType(typeof(DomiciliacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DomiciliacionResponse?>> DesactivateMyDomiciliacion(string domiciliacionGuid)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            var domiciliacion = await _domiciliacionService.DesactivateMyDomiciliacionAsync(userAuth, domiciliacionGuid);
            if (domiciliacion != null) return Ok(domiciliacion);
            return BadRequest(new { message = $"No se ha encontrado domiciliacion con guid {domiciliacionGuid}"});
        }
        catch (MovimientoNoPertenecienteAlUsuarioAutenticadoException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (MovimientoException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }
}