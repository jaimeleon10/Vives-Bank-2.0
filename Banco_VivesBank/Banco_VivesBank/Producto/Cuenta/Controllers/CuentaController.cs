using System.Numerics;
using System.Security.Claims;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Cuenta.Controllers;

[ApiController]
[Route("api/cuentas")]
[Produces("application/json")] 
[Tags("Cuentas")] 
public class CuentaController : ControllerBase
{

    private readonly ICuentaService _cuentaService;
    private readonly PaginationLinksUtils _paginationLinksUtils;
    private readonly IUserService _userService;



    public CuentaController(ICuentaService cuentaService, PaginationLinksUtils paginationLinksUtils,IUserService userService)
    {
        _cuentaService = cuentaService;
        _paginationLinksUtils = paginationLinksUtils;
        _userService = userService;
    }

    /// <summary>
    /// Obtiene todas las cuentas paginadas y filtradas por diferentes parámetros.
    /// </summary>
    /// <param name="saldoMax">Saldo máximo de las cuentas a filtrar</param>
    /// <param name="saldoMin">Saldo mínimo de las cuentas a filtrar</param>
    /// <param name="tipoCuenta">Tipo de cuenta a filtrar</param>
    /// <param name="page">Número de página a la que se quiere acceder</param>
    /// <param name="size">Número de cuentas por página</param>
    /// <param name="sortBy">Parámetro por el que se ordenan las cuentas</param>
    /// <param name="direction">Dirección de ordenación, ascendente (ASC) o descendente (DES)</param>
    /// <returns>Devuelve un ActionResult junto con una lista de PageResponse con las cuentas filtradas</returns>
    /// <response code="200">Devuelve una lista de cuentas paginadas</response>
    /// <response code="400">Ha ocurrido un error al obtener las cuentas</response>
    [HttpGet("admin")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PageResponse<CuentaResponse>>> Getall(
        [FromQuery] double? saldoMax = null,
        [FromQuery] double? saldoMin = null,
        [FromQuery] string? tipoCuenta = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "Id",
        [FromQuery] string direction = "asc")
    {
      
            var pageRequest = new PageRequest
            {
                PageNumber = page,
                PageSize = size,
                SortBy = sortBy,
                Direction = direction
            };

            var pageResult = await _cuentaService.GetAllAsync(saldoMax, saldoMin, tipoCuenta, pageRequest);

            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);

            Response.Headers.Append("link", linkHeader);

            return Ok(pageResult);
        
    }
    
    /// <summary>
    /// Obtiene todas las cuentas asociadas a un cliente específico mediante su GUID.
    /// </summary>
    /// <param name="guid">GUID del cliente cuyos cuentas se desean obtener</param>
    /// <returns>Devuelve un ActionResult con una lista de cuentas asociadas al cliente</returns>
    /// <response code="200">Devuelve una lista de cuentas asociadas al cliente</response>
    /// <response code="404">No se ha encontrado un cliente con el GUID especificado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("admin/cliente/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllByClientGuid(string guid)
    {
        try
        {
            var cuentas = await _cuentaService.GetByClientGuidAsync(guid);
            return Ok(cuentas);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound(new { message = "Cliente no encontrado.", details = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }


    /// <summary>
    /// Busca una cuenta por su GUID.
    /// </summary>
    /// <param name="guid">GUID, identificador único de la cuenta</param>
    /// <remarks>Se debe proporcionar un GUID válido. Si la cuenta no existe, se devolverá una respuesta 404 (No encontrado).</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta buscada</returns>
    /// <response code="200">Devuelve los datos de la cuenta con el GUID especificado</response>
    /// <response code="404">No se ha encontrado una cuenta con el GUID especificado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("admin/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> GetByGuid(string guid)
    {
        try
        { 
            var cuentaByGuid = await _cuentaService.GetByGuidAsync(guid);
            if (cuentaByGuid is null) return NotFound(new { message = $"Cuenta no encontrada con guid {guid}" });
            
            return Ok(cuentaByGuid);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    
    /// <summary>
    /// Busca una cuenta por su IBAN.
    /// </summary>
    /// <param name="iban">IBAN, identificador único de la cuenta</param>
    /// <remarks>Se debe proporcionar un IBAN válido. Si la cuenta no existe, se devolverá una respuesta 404 (No encontrado).</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta buscada</returns>
    /// <response code="200">Devuelve los datos de la cuenta con el IBAN especificado</response>
    /// <response code="404">No se ha encontrado una cuenta con el IBAN especificado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("admin/iban/{iban}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> GetByIban(string iban)
    {
        try
        {
            var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
            if (cuentaByIban is null) return NotFound(new { message = $"Cuenta no encontrada con IBAN {iban}" });
            
            return Ok(cuentaByIban);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    
    /// <summary>
    /// Elimina una cuenta por su GUID.
    /// </summary>
    /// <param name="guid">GUID, identificador único de la cuenta</param>
    /// <remarks>Se debe proporcionar un GUID válido. Si la cuenta no existe, se devolverá una respuesta 404 (No encontrado).</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta eliminada</returns>
    /// <response code="200">Devuelve los datos de la cuenta eliminada</response>
    /// <response code="404">No se ha encontrado una cuenta con el GUID especificado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("admin/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> DeleteAdmin(string guid)
    {
        try
        {
            var cuentaDelete = await _cuentaService.DeleteByGuidAsync(guid);
            if (cuentaDelete is null) return NotFound(new { message = $"Cuenta no encontrada con guid {guid}" });
            
            return Ok(cuentaDelete);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    
    /// <summary>
    /// Obtiene todas las cuentas asociadas al usuario autenticado.
    /// </summary>
    /// <returns>Devuelve un ActionResult con una lista de cuentas del usuario autenticado</returns>
    /// <response code="200">Devuelve una lista de cuentas del usuario autenticado</response>
    /// <response code="401">El usuario no está autenticado</response>
    /// <response code="404">No se han encontrado cuentas asociadas al usuario</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllMeAccounts()
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return Unauthorized(new { message = "No se ha podido identificar al usuario logeado" });

            var cuentas = await _cuentaService.GetAllMeAsync(userAuth.Guid);
            return Ok(cuentas);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    

    /// <summary>
    /// Busca una cuenta del usuario autenticado por su IBAN.
    /// </summary>
    /// <param name="iban">IBAN, identificador único de la cuenta</param>
    /// <remarks>Se debe proporcionar un IBAN válido. Si la cuenta no pertenece al usuario autenticado o no existe, se devolverá una respuesta 404 (No encontrado).</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta buscada</returns>
    /// <response code="200">Devuelve los datos de la cuenta con el IBAN especificado</response>
    /// <response code="401">El usuario no está autenticado</response>
    /// <response code="404">No se ha encontrado una cuenta con el IBAN especificado o la cuenta no pertenece al usuario</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("me/iban/{iban}")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> GetMeByIban(string iban)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null)  return Unauthorized(new { message = "No se ha podido identificar al usuario logeado" });

            var cuentaByIban = await _cuentaService.GetMeByIbanAsync(userAuth.Guid, iban);
            if (cuentaByIban is null) return NotFound(new { message = $"Cuenta no encontrada con IBAN {iban}" });

            return Ok(cuentaByIban);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    
    /// <summary>
    /// Crea una nueva cuenta para el usuario autenticado.
    /// </summary>
    /// <param name="cuentaRequest">Objeto que contiene los datos necesarios para crear la cuenta</param>
    /// <remarks>El usuario debe estar autenticado para crear una cuenta. Si el tipo de producto no existe o el cliente no es válido, se devolverá un error.</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta creada</returns>
    /// <response code="200">Devuelve los datos de la cuenta creada</response>
    /// <response code="401">El usuario no está autenticado</response>
    /// <response code="404">No se ha encontrado el cliente o el tipo de producto</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("me")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> Create([FromBody] CuentaRequest cuentaRequest)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null)  return Unauthorized(new { message = "No se ha podido identificar al usuario logeado" });

            var cuenta = await _cuentaService.CreateAsync(userAuth.Guid, cuentaRequest);
            return Ok(cuenta);
        }
        catch (ProductoNotExistException e)
        {
            return NotFound(new { message = "Tipo de producto no existente.", details = e.Message });
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound(new { message = "Cliente no encontrado.", details = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

    
    /// <summary>
    /// Elimina una cuenta del usuario autenticado por su GUID.
    /// </summary>
    /// <param name="guid">GUID, identificador único de la cuenta</param>
    /// <remarks>El usuario debe estar autenticado y la cuenta debe pertenecer a él. Si la cuenta no existe o no pertenece al usuario, se devolverá un error.</remarks>
    /// <returns>Devuelve un ActionResult con los datos de la cuenta eliminada</returns>
    /// <response code="200">Devuelve los datos de la cuenta eliminada</response>
    /// <response code="401">El usuario no está autenticado</response>
    /// <response code="403">El usuario no tiene permisos para eliminar esta cuenta</response>
    /// <response code="404">No se ha encontrado la cuenta con el GUID especificado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("me/{guid}")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CuentaResponse>> Delete(string guid)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return Unauthorized(new { message = "No se ha podido identificar al usuario logeado" });
            
            var cuentaDelete = await _cuentaService.DeleteMeAsync(userAuth.Guid, guid);
            if (cuentaDelete is null)  return NotFound(new { message = $"Cuenta no encontrada con guid {guid}" });
            
            return Ok(cuentaDelete);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound(new { message = "Cliente no encontrado.", details = e.Message });
        }
        catch (CuentaNoPertenecienteAlUsuarioException e)
        {
            return StatusCode(403, new { message = "No tienes permisos para eliminar esta cuenta.", details = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Error interno del servidor.", details = e.Message });
        }
    }

}