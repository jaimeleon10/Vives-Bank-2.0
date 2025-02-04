using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.User.Service;

public interface IUserService
{
    public Task<PageResponse<UserResponse>> GetAllAsync(string? username, Role? role, PageRequest pageRequest);
    public Task<IEnumerable<Models.User>> GetAllForStorage();
    public Task<UserResponse?> GetByGuidAsync(string guid);
    public Task<UserResponse?> GetByUsernameAsync(string username);
    public Task<Models.User?> GetUserModelByGuidAsync(string guid);
    public Task<Models.User?> GetUserModelByIdAsync(long id);
    public Task<UserResponse?> GetMeAsync(User.Models.User userAuth);
    
    public Task<UserResponse> CreateAsync(UserRequest userRequest);
    
    public Task<UserResponse?> UpdateAsync(string guid, UserRequestUpdate userRequestUpdate);
    public Task<UserResponse> UpdatePasswordAsync(Models.User user, UpdatePasswordRequest updatePasswordRequest);
    
    public Task<UserResponse?> DeleteByGuidAsync(string guid);
    
    string Authenticate(string username, string password);
    Models.User? GetAuthenticatedUser();
}