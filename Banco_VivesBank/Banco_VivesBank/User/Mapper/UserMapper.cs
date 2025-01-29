﻿using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Models;

namespace Banco_VivesBank.User.Mapper
{
    public static class UserMapper
    {
        public static Models.User ToModelFromRequest(this UserRequest userRequest)
        {
            return new Models.User
            {
                Username = userRequest.Username,
                Password = userRequest.Password
            };
        }


        public static Models.User ToModelFromEntity(this UserEntity userEntity)
        {
            return new Models.User
            {
                Id = userEntity.Id,
                Guid = userEntity.Guid,
                Username = userEntity.Username,
                Password = userEntity.Password,
                Role = userEntity.Role,
                CreatedAt = userEntity.CreatedAt,
                UpdatedAt = userEntity.UpdatedAt,
                IsDeleted = userEntity.IsDeleted
            };
        }
        
        public static UserEntity ToEntityFromModel(this Models.User usuario)
        {
            return new UserEntity
            {
                Id = usuario.Id,
                Guid = usuario.Guid,
                Username = usuario.Username,
                Password = usuario.Password,
                Role = usuario.Role,
                CreatedAt = usuario.CreatedAt,
                UpdatedAt = usuario.UpdatedAt,
                IsDeleted = usuario.IsDeleted
            };
        }

        public static UserResponse ToResponseFromModel(this Models.User usuario)
        {
            return new UserResponse
            {
                Guid = usuario.Guid,
                Username = usuario.Username,
                Password = usuario.Password,
                Role = usuario.Role.ToString(),
                CreatedAt = usuario.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
                UpdatedAt = usuario.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
                IsDeleted = usuario.IsDeleted
            };
        }
        
        public static UserResponse ToResponseFromEntity(this UserEntity userEntity)
        {
            return new UserResponse
            {
                Guid = userEntity.Guid,
                Username = userEntity.Username,
                Password = userEntity.Password,
                Role = userEntity.Role.ToString(),
                CreatedAt = userEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
                UpdatedAt = userEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
                IsDeleted = userEntity.IsDeleted
            };
        }
        
        public static IEnumerable<UserResponse> ToResponseListFromEntityList(this IEnumerable<UserEntity> userEntityList)
        {
            return userEntityList.Select(userEntity => ToResponseFromEntity(userEntity));
        }
    }
}
