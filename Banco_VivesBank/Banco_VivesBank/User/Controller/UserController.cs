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
            return Unauthorized(e.Message);
        }
    }
    
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
            return StatusCode(404, new { message = "No se han encontrado los usuarios.", details = e.Message });

        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }

    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> GetByGuid(string guid)
    {
        var user = await _userService.GetByGuidAsync(guid);

        if (user is null) return NotFound($"No se ha encontrado usuario con guid: {guid}");

        return Ok(user); 
    }
    
    [HttpGet("me")]
    [Authorize(Policy = "ClienteOrUserPolicy")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
        
        var user = await _userService.GetMeAsync(userAuth);

        if (user is null) return NotFound($"No se ha encontrado usuario con guid: {userAuth.Guid}");

        return Ok(user); 
    }
    
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> GetByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);

        if (user is null) return NotFound($"No se ha encontrado usuario con nombre de usuario: {username}"); 
        return Ok(user); 
    }
    
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
            return BadRequest(e.Message);
        }
    }
    
    [HttpPut("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> Update(string guid, [FromBody] UserRequestUpdate userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userResponse = await _userService.UpdateAsync(guid, userRequest);
        if (userResponse is null) return NotFound($"No se ha podido actualizar el usuario con guid: {guid}"); 
        return Ok(userResponse);
    }
    
    [HttpPut("password")]
    [Authorize(Policy = "ClienteOrUserPolicy")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
            var updatedUser = await _userService.UpdatePasswordAsync(userAuth, updatePasswordRequest);
            return Ok(updatedUser);
        }
        catch (UserException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<UserResponse>> DeleteByGuid(string guid)
    {
        var userResponse = await _userService.DeleteByGuidAsync(guid);
        if (userResponse is null) return NotFound($"No se ha podido borrar el usuario con guid: {guid}"); 
        return Ok(userResponse);
    }
}