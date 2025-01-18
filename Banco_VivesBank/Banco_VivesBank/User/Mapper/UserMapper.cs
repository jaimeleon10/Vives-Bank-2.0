using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Models;

namespace Banco_VivesBank.User.Mapper
{
    public class UserMapper
    {
        public static Models.User ToModelFromRequest(UserRequest userRequest)
        {
            return new Models.User
            {
                Username = userRequest.Username,
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
                Username = userEntity.Username,
                Password = userEntity.Password,
                Role = userEntity.Role,
                IsDeleted = userEntity.IsDeleted,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt
            };
        }
        
        public static UserEntity ToEntityFromModel(Models.User usuario)
        {
            return new UserEntity
            {
                Id = usuario.Id,
                Guid = usuario.Guid,
                Username = usuario.Username,
                Password = usuario.Password,
                Role = usuario.Role,
                IsDeleted = usuario.IsDeleted,
                CreatedAt = usuario.CreatedAt,
                UpdatedAt = usuario.UpdatedAt
            };
        }

        public static UserResponse ToResponseFromModel(Models.User usuario)
        {
            return new UserResponse
            {
                Guid = usuario.Guid,
                Username = usuario.Username,
                Role = usuario.Role.ToString(),
                CreatedAt = usuario.CreatedAt,
                UpdatedAt = usuario.UpdatedAt,
                IsDeleted = usuario.IsDeleted
            };
        }
        
        public static UserResponse ToResponseFromEntity(UserEntity userEntity)
        {
            return new UserResponse
            {
                Guid = userEntity.Guid,
                Username = userEntity.Username,
                Role = userEntity.Role.ToString(),
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt,
                IsDeleted = userEntity.IsDeleted
            };
        }
        
        public static IEnumerable<UserResponse> ToResponseListFromEntityList(IEnumerable<UserEntity> userEntityList)
        {
            return userEntityList.Select(userEntity => ToResponseFromEntity(userEntity));
        }
    }

}
