using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.User.Controller;
    
[ApiController]
[Route("api/usuarios")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly PaginationLinksUtils _paginationLinksUtils;

    public UserController(IUserService userService,PaginationLinksUtils paginationLinksUtils)
    {
        _userService = userService;
        _paginationLinksUtils = paginationLinksUtils;
    }
    
    [HttpPost("login")]
    public ActionResult Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var token = _userService.Authenticate(loginRequest.Username, loginRequest.Password);
            return Ok(new { Token = token });
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(new { message = e.Message});
        }
    }
    /// <summary>
    /// Obtiene todos los usuarios paginados y filtrados por diferentes parámetros.
    /// </summary>
    /// <remarks>El usuario debe ser un administrador</remarks>
    /// <param name="username">Nombre de usuario de los usuarios a filtrar</param>
    /// <param name="role">Rol de los usuarios a filtrar</param>
    /// <param name="page">Número de página a la que se quiere acceder</param>
    /// <param name="size">Número de usuarios que puede haber por página</param>
    /// <param name="sortBy">Parametro por la que se ordenan los usuarios</param>
    /// <param name="direction">Dirección de ordenación, ascendiente(ASC) o descendiente (DES)</param>
    /// <returns>Devuelve un ActionResult junto con una lista de PageResponse con los usuarios filtrados</returns>
    /// <response code="200">Devuelve una lista de usuarios paginados</response>
    /// <response code="400">No se han encontrado usuarios</response>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<UserResponse>>> Getall(
        [FromQuery] string? username = null,
        [FromQuery] Role? role = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "Id",
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

            var pageResult = await _userService.GetAllAsync(username, role, pageRequest);

            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);

            Response.Headers.Append("link", linkHeader);

            return Ok(pageResult);
        }
        catch (UserNotFoundException e)
        {
            return NotFound(new { message = "No se han encontrado los usuarios.", details = e.Message });

        }
        catch (Exception e)
        {
            return NotFound(new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }
    /// <summary>
    /// Obtiene un usuario por su guid
    /// </summary>
    /// <param name="guid">Guid del usuario a buscar</param>
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> GetByGuid(string guid)
    {
        var user = await _userService.GetByGuidAsync(guid);

        if (user is null) return NotFound(new { message = $"No se ha encontrado usuario con guid: {guid}"});

        return Ok(user); 
    }
    /// <summary>
    /// Obtiene el usuario logeado
    /// </summary>
    /// <param name="guid">Guid del usuario a buscar</param>
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpGet("me")]
    [Authorize(Policy = "ClienteOrUserPolicy")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
        
        var user = await _userService.GetMeAsync(userAuth);

        if (user is null) return NotFound(new { message = $"No se ha encontrado usuario con guid: {userAuth.Guid}"});

        return Ok(user); 
    }
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    /// <param name="username">Nombre de usuario del usuario a buscar</param>
    /// <remarks>
    ///Se debe proporcionar un nombre de usuario válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> GetByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);

        if (user is null) return NotFound(new { message = $"No se ha encontrado usuario con nombre de usuario: {username}"}); 
        return Ok(user); 
    }
    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="userRequest">Datos del usuario a crear</param>
    /// <remarks>
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la craecion del usuario con guid especificado</response>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserRequest userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _userService.CreateAsync(userRequest));
        }
        catch (UserException e)
        {
            return BadRequest(new { message = e.Message});
        }
    }
    /// <summary>
    /// Actualiza un usuario por su guid
    /// </summary>
    /// <param name="guid">Guid del usuario a buscar</param>
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpPut("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> Update(string guid, [FromBody] UserRequestUpdate userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userResponse = await _userService.UpdateAsync(guid, userRequest);
        if (userResponse is null) return NotFound(new { message = $"No se ha podido actualizar el usuario con guid: {guid}"}); 
        return Ok(userResponse);
    }
    /// <summary>
    /// Actualiza la contraseña del usuario logeado
    /// </summary>
    /// <param name="updatePasswordRequest">Datos de la nueva contraseña</param>
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpPut("password")]
    [Authorize(Policy = "ClienteOrUserPolicy")]
    public async Task<ActionResult> UpdateMyPassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new { message = "No se ha podido identificar al usuario logeado"});
            var updatedUser = await _userService.UpdatePasswordAsync(userAuth, updatePasswordRequest);
            return Ok(updatedUser);
        }
        catch (UserException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Borra un usuario por su guid
    /// </summary>
    /// <param name="guid">Guid del usuario a buscar</param>
    /// <remarks>
    ///Se debe proporcionar un Guid válido y el usuario debe ser un administrador
    /// </remarks>
    /// <returns>Devuelve una respuesta https junto a un UserResponse con los datos del usuario a buscar</returns>
    /// <response code="200">Devuelve un ActionResult con lo datos del usuario con el guid especificado</response>
    /// <response code="404">No se ha encontrado usuario con el guid especificado</response>
    /// <response code="400">Ha ocurrido un error durante la busqueda del usuario con guid especificado</response>
    [HttpDelete("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> DeleteByGuid(string guid)
    {
        var userResponse = await _userService.DeleteByGuidAsync(guid);
        if (userResponse is null) return NotFound(new { message = $"No se ha podido borrar el usuario con guid: {guid}"}); 
        return Ok(userResponse);
    }
}