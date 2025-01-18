using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.User.Controller;
    
[ApiController]
[Route("api/usuarios")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        return Ok(await _userService.GetAllAsync());
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<UserResponse>> GetByGuid(string guid)
    {
        var user = await _userService.GetByGuidAsync(guid);

        if (user is null) return NotFound($"No se ha encontrado usuario con guid: {guid}");

        return Ok(user); 
    }
    
    [HttpGet("/username/{username}")]
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
    public async Task<ActionResult<UserResponse>> Update(string guid, [FromBody] UserRequest userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userResponse = await _userService.UpdateAsync(guid, userRequest);
        if (userResponse is null) return NotFound($"No se ha podido actualizar el usuario con guid: {guid}"); 
        return Ok(userResponse);
    }
    
    [HttpDelete("{guid}")]
    public async Task<ActionResult<UserResponse>> DeleteByGuid(string guid)
    {
        var userResponse = await _userService.DeleteByGuidAsync(guid);
        if (userResponse is null) return NotFound($"No se ha podido borrar el usuario con guid: {guid}"); 
        return Ok(userResponse);
    }
}