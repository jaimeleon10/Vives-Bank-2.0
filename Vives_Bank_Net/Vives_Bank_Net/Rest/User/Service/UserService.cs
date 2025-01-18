﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vives_Bank_Net.Rest.Database;
using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dto;
using Vives_Bank_Net.Rest.User.Exceptions;
using Vives_Bank_Net.Rest.User.Mapper;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Rest.User.Service
{
    public class UserService : IUserService 
    {    
        private const string CacheKeyPrefix = "User_";
        private readonly GeneralDbContext _context;
        private readonly ILogger<UserService> _logger;
        
        public UserService(GeneralDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los usuarios");
            try
            {
                var userEntityList = await _context.Usuarios.ToListAsync();
                if (userEntityList == null || userEntityList.Count == 0)
                {
                    throw new UserNotFoundException("No se han encontrado usuarios");
                }
                _logger.LogInformation("Usuarios obtenidos con éxito");
                var userModelList = UserMapper.ToModelListFromEntityList(userEntityList);

                return UserMapper.ToResponseListFromModelList(userModelList);
            }
            catch (UserNotFoundException e)
            {
                _logger.LogWarning(e, "No se han encontrado usuarios");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener todos los usuarios");
                throw new UserNotFoundException("Error al obtener todos los usuarios");
            }
        }

        public async Task<UserResponse?> GetByGuidAsync(string guid)
        {
            _logger.LogInformation($"Buscando Usuario con guid: {guid}");
            var cacheKey = CacheKeyPrefix + guid;

            /*if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }*/

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid.ToLower() == guid.ToLower());
            if (userEntity != null)
            {
                /*cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;*/
                
                var userModel = UserMapper.ToModelFromEntity(userEntity);
                return UserMapper.ToResponseFromModel(userModel);
            }

            return null;
        }

        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"Buscando Usuario con username: {username}");
            var cacheKey = CacheKeyPrefix + username;

            /*if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }*/

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.UserName == username);
            if (userEntity != null)
            {
                /*cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;*/
                
                var userModel = UserMapper.ToModelFromEntity(userEntity);
                return UserMapper.ToResponseFromModel(userModel);
            }

            return null;
        }

        public async Task<UserResponse> CreateAsync(UserRequest userRequest)
        {
            _logger.LogInformation("Creando Usuario");

            if (await _context.Usuarios.AnyAsync(u => u.UserName.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un usuario con el nombre: {userRequest.Username}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userRequest.Username}");
            }

            var userModelRequest = UserMapper.ToModelFromRequest(userRequest);
            _context.Usuarios.Add(UserMapper.ToEntityFromModel(userModelRequest));
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + userEntity.Id;
            _memoryCache.Set(cacheKey, UserMapper.ToModelFromEntity(userEntity), TimeSpan.FromMinutes(30));
            */

            _logger.LogInformation("Usuario creado con éxito");
            return UserMapper.ToResponseFromModel(userModelRequest);
        }


        public async Task<UserResponse?> UpdateAsync(string guid, UserRequest userRequest)
        {
            _logger.LogInformation($"Actualizando Usuario con guid: {guid}");
            
            var userEntityExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid.ToLower() == guid.ToLower());
            if (userEntityExistente == null)
            {
                _logger.LogWarning($"Usuario no encontrado con guid: {guid}");
                return null;
            }

            if (userRequest.Username != userEntityExistente.UserName && await _context.Usuarios.AnyAsync(u => u.UserName.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un Usuario con el nombre: {userRequest.Username}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userRequest.Username}");
            }

            userEntityExistente.UserName = userRequest.Username;
            userEntityExistente.Password = userRequest.Password;
            userEntityExistente.Role = Enum.Parse<Role>(userRequest.Role, true);
            userEntityExistente.UpdatedAt = DateTime.UtcNow;
            userEntityExistente.IsDeleted = userRequest.IsDeleted;

            _context.Usuarios.Update(userEntityExistente);
            await _context.SaveChangesAsync();
            
            /*
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
            */
            
            _logger.LogInformation($"Usuario actualizado con guid: {guid}");
            var userModel = UserMapper.ToModelFromEntity(userEntityExistente);
            return UserMapper.ToResponseFromModel(userModel);
        }

        public async Task<UserResponse?> DeleteByIdAsync(string guid)
        {
            _logger.LogInformation($"Borrando Usuario con guid: {guid}");
            
            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid.ToLower() == guid.ToLower());
            if (userEntity == null)
            {
                _logger.LogWarning($"Usuario no encontrado con guid: {guid}");
                return null;
            }

            userEntity.IsDeleted = true;
            userEntity.UpdatedAt = DateTime.UtcNow;

            _context.Usuarios.Update(userEntity);
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
            */

            _logger.LogInformation($"Usuario borrado (desactivado) con id: {guid}");
            var userModel = UserMapper.ToModelFromEntity(userEntity);
            return UserMapper.ToResponseFromModel(userModel);
        }
    }
}
