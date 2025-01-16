using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dtos;

namespace Vives_Bank_Net.Rest.User.Service;

public interface IUserService
{
    public Task<List<User>> GetAllAsync();
    public Task<User?> GetByIdAsync(string id);
    public Task<User?> GetByUsernameAsync(string username);
    public Task<UserResponse> CreateAsync(UserEntity userEntity);
    public Task<UserResponse?> UpdateAsync(string id, UserRequestDto userRequest);
    public Task<UserResponse?> DeleteByIdAsync(string id);
}