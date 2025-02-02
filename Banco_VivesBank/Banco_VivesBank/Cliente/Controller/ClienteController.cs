using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Pdf.Services;
using Banco_VivesBank.Swagger.Examples.Clientes;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Cliente.Controller;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")] 
[Tags("Clientes")] 
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IPdfStorage _pdfStorage;
    private readonly IMovimientoService _movimientoService;
    private readonly PaginationLinksUtils _paginationLinksUtils;
    private readonly IUserService _userService;

    public ClienteController(IClienteService clienteService, PaginationLinksUtils paginations
        , IPdfStorage pdfStorage, IMovimientoService movimientoService, IUserService userService)
    {
        _clienteService = clienteService;
        _paginationLinksUtils = paginations;
        _pdfStorage = pdfStorage;
        _movimientoService = movimientoService;
        _userService = userService;
    }
    
    /// <summary>
    /// Obtiene todos los clientes paginados y filtrados por diferentes parámetros.
    /// </summary>
    /// <param name="nombre">Nombre de los clientes a filtrar</param>
    /// <param name="apellido">Apellidos de los clientes a filtrar</param>
    /// <param name="dni">Dni por el que se quiere filtrar </param>
    /// <param name="page">Número de página a la que se quiere acceder</param>
    /// <param name="size">Número de clientes que puede haber por página</param>
    /// <param name="sortBy">Parametro por la que se ordenan los clientes</param>
    /// <param name="direction">Dirección de ordenación, ascendiente(ASC) o descendiente (DES)</param>
    /// <returns>Devuelve un ActionResult junto con una lista de PageResponse con los clientes filtrados</returns>
    /// <response code="200">Devuelve una lista de clientes paginados</response>
    /// <response code="400">No se han encontrado clientes</response>
    [HttpGet]
    [ProducesResponseType(typeof(PageResponseClienteExample), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<List<PageResponse<ClienteResponse>>>> GetAllPaged(
        [FromQuery] string? nombre = null,
        [FromQuery] string? apellido = null,
        [FromQuery] string? dni = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "id",
        [FromQuery] string direction = "asc")
    {
        try
        {
            var pageRequest = new PageRequest
            {
                PageNumber = page,
                PageSize = size,
                SortBy = sortBy,
                Direction = direction
            };
            var pageResult = await _clienteService.GetAllPagedAsync(nombre, apellido, dni, pageRequest);
            
            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
            Response.Headers.Append("link", linkHeader);
            
            return Ok(pageResult);

        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = "No se han encontrado clientes.", details = e.Message });
        }
    }
    
    /// <summary>
    /// Busca un cliente por su Guid
    /// </summary>
    /// <param name="guid">Guid, identificador único del cliente</param>
    /// <remarks>Se debe proporcionar un Guid válido. Si el cliente no existe, se devolvera un null, una respuesta 404, no encontrado</remarks>
    /// <returns>Devuelve un typeOf(ActionResult) junto a un typeOf(ClienteResponse) con los datos del cliente a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del cliente con el guid especificado</response>
    /// <response code="404">No se ha encontrado cliente con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del cliente con guid especificado</response>
    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(ClienteResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponse>> GetByGuid(string guid)
    {
        try
        {
            var cliente = await _clienteService.GetByGuidAsync(guid);
     
            if (cliente is null) return NotFound(new { message = $"No se ha encontrado cliente con guid: {guid}"});
        
            return Ok(cliente);
        }
        catch (Exception e)
        {
            return BadRequest(new { message = $"Ha ocurrido un error durante la busqueda del cliente con guid {guid}", details = e.Message });
        }
        
    }
    
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> GetMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var cliente = await _clienteService.GetMeAsync(userAuth);
     
        if (cliente is null) return NotFound(new { message = $"No se ha encontrado el cliente autenticado"});
        
        return Ok(cliente);
    }

    [HttpPost]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<ClienteResponse>> Create([FromBody] ClienteRequest clienteRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _clienteService.CreateAsync(userAuth, clienteRequest));
        }
        catch (ClienteException e)
        {
            return BadRequest(new { message = e.Message});
        }
        catch (UserException e)
        {
            return NotFound(new { message = e.Message});
        }
    }
    
    [HttpPut]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> UpdateMe([FromBody] ClienteRequestUpdate clienteRequestUpdate)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
            
            var clienteResponse = await _clienteService.UpdateMeAsync(userAuth, clienteRequestUpdate);
            if (clienteResponse is null) return NotFound(new { message = $"No se ha podido actualizar el cliente autenticado"}); 
            return Ok(clienteResponse);
        }
        catch (ClienteException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }

    [HttpDelete("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ClienteResponse>> DeleteByGuid(string guid)
    {
        var clienteResponse = await _clienteService.DeleteByGuidAsync(guid);
        if (clienteResponse is null) return NotFound(new { message = $"No se ha podido borrar el usuario con guid: {guid}"}); 
        return Ok(clienteResponse);
    }
    
    [HttpDelete]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> DeleteMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var clienteResponse = await _clienteService.DeleteMeAsync(userAuth);
        if (clienteResponse is null) return NotFound(new { message = $"No se ha podido borrar el cliente autenticado"}); 
        return Ok(clienteResponse);
    }

    [HttpPatch("fotoPerfil")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> UpdateMyProfilePicture(IFormFile foto)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            var clienteResponse = await _clienteService.UpdateFotoPerfil(userAuth, foto);

            if (clienteResponse is null)
                return NotFound(new { message = $"No se ha podido actualizar la foto de perfil del cliente autenticado"});

            return Ok(clienteResponse);
        }
        catch (FileStorageException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("fotoDni")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> UpdateMyDniPicture(IFormFile foto)
    {
        if (foto == null || foto.Length == 0)
        {
            return BadRequest("Debe proporcionar una foto válida para actualizar el DNI.");
        }
        
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            
            var clienteResponse = await _clienteService.UpdateFotoDni(userAuth, foto);
            return Ok(clienteResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Ocurrió un error al actualizar la foto del DNI", details = ex.Message});
        }
    }

    [HttpGet("fotoDni/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetFotoDni(string guid)
    {
        try
        {
            var fotoStream = await _clienteService.GetFotoDniAsync(guid);
        
            if (fotoStream == null)
                return NotFound(new { message = "No se encontró la foto del DNI para este cliente."});

            return File(fotoStream, "image/jpeg");
        }
        catch (Exception ex)
        {
            return NotFound(new { message = "Error al recuperar la foto del DNI.", details = ex.Message });
        }
    }
    
    [HttpGet("movimientosPDF")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<List<MovimientoResponse>>> GetMovimientos()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var clienteResponse = await _clienteService.GetMeAsync(userAuth);
        if (clienteResponse is null)
            return NotFound(new { message = $"No se ha encontrado el cliente autenticado"});

        var movimientos = await _movimientoService.GetByClienteGuidAsync(clienteResponse.Guid);

        if (movimientos == null || !movimientos.Any()) return NotFound(new { message = $"No se han encontrado movimientos del cliente con guid: {clienteResponse.Guid}"});

        var movimientosList = movimientos.ToList();

        _pdfStorage.ExportPDF(clienteResponse, movimientosList);
        return Ok(movimientosList);
    }

}
