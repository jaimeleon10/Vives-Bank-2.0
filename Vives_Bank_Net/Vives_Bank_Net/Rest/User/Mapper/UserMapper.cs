using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dto;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Rest.User.Mapper
{
    public class UserMapper
    {
        public static Models.User ToModelFromRequest(UserRequest userRequest)
        {
            return new Models.User
            {
                UserName = userRequest.Username,
                Password = userRequest.Password,
                Role = (Role)Enum.Parse(typeof(Role), userRequest.Role),
                IsDeleted = userRequest.IsDeleted
            };
        }


        public static Models.User ToModelFromEntity(UserEntity userEntity)
        {
            return new Models.User
            {
                Id = userEntity.Id,
                Guid = userEntity.Guid,
                UserName = userEntity.UserName,
                Password = userEntity.Password,
                Role = userEntity.Role,
                IsDeleted = userEntity.IsDeleted,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt
            };
        }
        
        public static UserEntity ToEntityFromModel(Models.User user)
        {
            return new UserEntity
            {
                Id = user.Id,
                Guid = user.Guid,
                UserName = user.UserName,
                Password = user.Password,
                Role = user.Role,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public static UserResponse ToResponseFromModel(Models.User user)
        {
            return new UserResponse
            {
                Guid = user.Guid,
                Username = user.UserName,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsDeleted = user.IsDeleted
            };
        }
        
        public static IEnumerable<Models.User> ToModelListFromEntityList(IEnumerable<UserEntity> userEntityList)
        {
            return userEntityList.Select(userEntity => ToModelFromEntity(userEntity));
        }
        
        public static IEnumerable<UserResponse> ToResponseListFromModelList(IEnumerable<Models.User> userList)
        {
            return userList.Select(user => ToResponseFromModel(user));
        }
    }

}
