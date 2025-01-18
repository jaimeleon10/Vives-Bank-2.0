using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dto;

namespace Vives_Bank_Net.Rest.User.Service;

public interface IUserService
{
    public Task<IEnumerable<UserResponse>> GetAllAsync();
    public Task<UserResponse?> GetByGuidAsync(string guid);
    public Task<UserResponse?> GetByUsernameAsync(string username);
    public Task<UserResponse> CreateAsync(UserRequest userRequest);
    public Task<UserResponse?> UpdateAsync(string guid, UserRequest userRequest);
    public Task<UserResponse?> DeleteByIdAsync(string guid);
}