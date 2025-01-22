using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.User.Service;

public interface IUserService
{
    public Task<PageResponse<UserResponse>> GetAllAsync(
        string? username,
        Role? role,
        PageRequest pageRequest);
    public Task<UserResponse?> GetByGuidAsync(string guid);
    public Task<UserResponse?> GetByUsernameAsync(string username);
    public Task<Models.User?> GetUserModelByGuid(string guid);
    public Task<Models.User?> GetUserModelById(long id);
    public Task<UserResponse> CreateAsync(UserRequest userRequest);
    public Task<UserResponse?> UpdateAsync(string guid, UserRequest userRequest);
    public Task<UserResponse?> DeleteByGuidAsync(string guid);
}