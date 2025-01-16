using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dtos;

namespace Vives_Bank_Net.Rest.User.Mapper
{
    public class UserMapper
    {
        public static UserEntity ToEntity(UserRequestDto userDto)
        {
            return new UserEntity
            {
                UserName = userDto.Username,
                PasswordHash = userDto.PasswordHash,
                Role = Enum.GetName(typeof(Role), userDto.Role),
                IsDeleted = userDto.IsDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }


        public static User ToModelFromEntity(UserEntity userEntity)
        {
            return new User
            {
                Id = userEntity.Id,
                Guid = userEntity.Guid,
                UserName = userEntity.UserName,
                PasswordHash = userEntity.PasswordHash,
                Role = (Role)Enum.Parse(typeof(Role), userEntity.Role),
                IsDeleted = userEntity.IsDeleted,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt
            };
        }

        public static UserResponse ToUserResponseFromEntity(UserEntity userEntity)
        {
            return new UserResponse
            {
                Username = userEntity.UserName,
                Role = userEntity.Role,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt,
                IsDeleted = userEntity.IsDeleted
            };
        }
    }

}
