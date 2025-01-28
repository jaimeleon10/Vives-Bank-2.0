using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Utils.Jwt;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRequest userRequest)
    {
        _userService.RegisterUser(userRequest);
        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        var token = _userService.Authenticate(loginDto.Username, loginDto.Password);
        return Ok(new { Token = token });
    }
}