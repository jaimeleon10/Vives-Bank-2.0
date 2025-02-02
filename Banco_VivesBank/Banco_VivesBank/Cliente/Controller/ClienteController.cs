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
using Microsoft.IdentityModel.Tokens;

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
    /// <remarks>El usuario debe ser un administrador</remarks>
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
    [ProducesResponseType(typeof(PageResponse<ClienteResponse>), StatusCodes.Status200OK)]
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
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un ClienteResponse con los datos del cliente a buscar</returns>
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
    
    /// <summary>
    /// Permite a un usuario mostrar los datos de su cliente asociado
    /// </summary>
    /// <remarks>El usuario debe estar autenticado y tener un cliente relacionado</remarks>
    /// <returns>Una respuesta http junto con los datos del cliente</returns>
    /// <response code="200">Devuelve los datos del cliente autenticado</response>
    /// <response code="404">No se ha encontrado a un cliente relacionado con el usuario o no se ha identificado al usuario</response>
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> GetMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var cliente = await _clienteService.GetMeAsync(userAuth);
     
        if (cliente is null) return NotFound(new { message = $"No se ha encontrado el cliente autenticado"});
        
        return Ok(cliente);
    }
    
    /// <summary>
    ///  Crea un nuevo cliente en la base de datos a partir de los datos proporcionados
    /// </summary>
    /// <remarks>
    /// Se crea el cliente si los datos proporcionados en el ClienteRequest son válidos y si el usuario está autenticado
    ///</remarks>
    /// <param name="clienteRequest"> Datos del cliente a crear</param>
    /// <returns> Devuelve un ActionResult con el ClienteResponse creado</returns>
    /// <response code="200">Devuelve el cliente creado</response>
    /// <response code="400">Si la estructura del body es incorrecto o si algún dato (Dni, teléfono, email o usuario) ya está asociado a un cliente.</response>
    /// <response code="404">Si no se ha podido identificar al usuario logueado</response>
    [HttpPost]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    /// <summary>
    ///  Permite a un usuario modificar su propio cliente
    /// </summary>
    /// <remarks> Se actualiza el cliente si los datos proporcionados en el ClienteRequestUpdate son válidos, si el cliente pertenece al usuario y si el usuario está autenticado</remarks>
    /// <param name="clienteRequestUpdate"> Datos a actualizar del cliente</param>
    /// <returns>El cliente con los nuevos datos actualizados</returns>
    /// <response code="200">Devuelve el cliente actualizado</response>
    /// <response code="400">Si la estructura del body es incorrecto o si algún dato (Dni, teléfono, email o usuario) ya está asociado a un cliente.</response>
    /// <response code = "404">Si no se ha podido identificar al usuario logueado o si no se ha encontrado al cliente que se quiere actualizar</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Borra un cliente a partir de su guid, si el usuario autenticado es un administrador
    /// </summary>
    /// <param name="guid">Identificador del cliente a borrar</param>
    /// <returns>Los datos del cliente que se desea borrar</returns>
    /// <response code="200">Devuelve el cliente borrado</response>
    /// <response code = "404">Si no se ha encontrado al cliente que se quiere borrar</response>
    [HttpDelete("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> DeleteByGuid(string guid)
    {
        var clienteResponse = await _clienteService.DeleteByGuidAsync(guid);
        if (clienteResponse is null) return NotFound(new { message = $"No se ha podido borrar el usuario con guid: {guid}"}); 
        return Ok(clienteResponse);
    }
    
    /// <summary>
    /// Permite a un usuario borrar su propio cliente
    /// </summary>
    /// <returns>Devuelve el cliente ya borrado</returns>
    /// <response code="200">Devuelve el cliente borrado</response>
    /// <response code = "404">Si no se ha encontrado al cliente que se quiere borrar o no se a podido identificar al usuario</response>
    [HttpDelete]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> DeleteMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var clienteResponse = await _clienteService.DeleteMeAsync(userAuth);
        if (clienteResponse is null) return NotFound(new { message = $"No se ha podido borrar el cliente autenticado"}); 
        return Ok(clienteResponse);
    }
    
    /// <summary>
    /// Permite a un usuario borrar su cliente asociado y todos los datos personales
    /// </summary>
    /// <returns>Devuelve un mensaje confirmando que se han borrado los datos</returns>
    /// <response code="200">Devuelve un mensaje confirmando que se han borrado los datos</response>
    /// <response code = "404">Si no se ha podido identificar al usuario logeado o no se ha encontrado a un cliente asociado</response>
    [HttpDelete ("DerechoAlOlvido")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> DerechoAlOlvido()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});

        try
        {
            var response = await _clienteService.DerechoAlOlvido(userAuth);
            if (response.IsNullOrEmpty())
                return NotFound(new { message = "No se ha encontrado ningun cliente asociado a su usuario" });
            return Ok(response);
        }
        catch (FileStorageException e)
        {
            return BadRequest(new
                { message = "Ha ocurrido un error al intentar borrar la imagen de perfil", details = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = "Ha ocurrido un error al intentar borrar la imagen del dni", details = e.Message });
        }
    }

    /// <summary>
    /// Cambia la foto de perfil del cliente autenticado
    /// </summary>
    /// <param name="foto">Archivo tiene que ser una imagen</param>
    /// <returns>Los datos del cliente con la foto modificada</returns>
    /// <response code="200">Devuelve el cliente con la foto de perfil actualizada</response>
    /// <response code = "404">Si no se ha encontrado al cliente que se quiere actualizar o no se ha podido identificar al usuario</response>
    /// <response code = "400">Si la foto no es válida o ha ocurrido algun error al almacenar la nueva foto</response>
    [HttpPatch("fotoPerfil")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    /// <summary>
    /// Modifica la foto del DNI del cliente autenticado
    /// </summary>
    /// <param name="foto">Imagen que se desea almacenar</param>
    /// <returns>Devuelve al cliente con los datos de la nueva imagen cambiada</returns>
    /// <response code="200">Devuelve el cliente con la foto del DNI actualizada</response>
    /// <response code = "404">Si no se ha encontrado al cliente que se quiere actualizar o no se ha podido identificar al usuario</response>
    /// <response code = "400">Si la foto no es válida o ha ocurrido algun error al almacenar la nueva foto</response>
    [HttpPost("fotoDni")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Recupera la foto de perfil a partir del guid, si el usuario autenticado es un administrador
    /// </summary>
    /// <param name="guid">Identificador del cliente</param>
    /// <returns>Recupera la imagen del dni del cliente</returns>
    /// <response code="200">Devuelve la imagen del dni del cliente</response>
    /// <response code = "404">Si no se ha encontrado la foto del dni del cliente</response>
    [HttpGet("fotoDni/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    /// <summary>
    /// Recupera una lista de movimientos hechos por un cliente y genera un PDF con ellos
    /// </summary>
    /// <returns>Un json con los movimiento y un pdf</returns>
    /// <response code="200">Devuelve una lista de movimientos y un pdf con ellos</response>
    /// <response code = "404">Si no se han encontrado movimientos del cliente o si no se ha encontrado al cliente o si no se ha podido identificar al usuario</response>
   
    [HttpGet("movimientosPDF")]
    [Authorize(Policy = "ClientePolicy")]
    [ProducesResponseType(typeof(ClienteResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
