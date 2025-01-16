using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.User.Dtos;
using Vives_Bank_Net.Rest.User.Exceptions;
using Vives_Bank_Net.Rest.User.Mapper;
using Vives_Bank_Net.Rest.User.Service;

namespace Vives_Bank_Net.Rest.User.Controller
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "No se han encontrado usuarios");
                return NotFound(ex.Message); // Devolver 404
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user is null) return NotFound($"User not found by id: {id}");

            return Ok(user); 
        }
        
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);

            if (user is null) return NotFound($"User not found by username: {username}"); 
            return Ok(user); 
        }
        
        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateAsync([FromBody] UserRequestDto userRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
    
            try
            {
                var userEntity = UserMapper.ToEntity(userRequestDto);
                var userResponse = await _userService.CreateAsync(userEntity);
                return Ok(userResponse);
            }
            catch (UserException e)
            {
                return BadRequest(e.Message);
            }
        }


        
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> UpdateAsync(string id, UserRequestDto userRequest)
        {
            var userResponse = await _userService.UpdateAsync(id, userRequest);
            if (userResponse is null) return NotFound($"User not found by id: {id}"); 
            return Ok(userResponse);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserResponse>> DeleteByIdAsync(string id)
        {
            var userResponse = await _userService.DeleteByIdAsync(id);
            if (userResponse is null) return NotFound($"User not found by id: {id}"); 
            return Ok(userResponse);
        }
    }
}
