using Banco_VivesBank.Database;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Models;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.User.Service
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
            var userEntityList = await _context.Usuarios.ToListAsync();
            return UserMapper.ToResponseListFromEntityList(userEntityList);
        }

        public async Task<UserResponse?> GetByGuidAsync(string guid)
        {
            _logger.LogInformation($"Buscando Usuario con guid: {guid}");
            
            /*
            var cacheKey = CacheKeyPrefix + guid;
            if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }
            */

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid.ToLower() == guid.ToLower());
            if (userEntity != null)
            {
                /*cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;*/

                return UserMapper.ToResponseFromEntity(userEntity);
            }

            return null;
        }

        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"Buscando Usuario con username: {username}");
            
            /*
            var cacheKey = CacheKeyPrefix + username;

            if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }
            */

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
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

            if (await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un usuario con el nombre: {userRequest.Username}");
                throw new UserExistException($"Ya existe un Usuario con el nombre: {userRequest.Username}");
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
            _logger.LogInformation($"Actualizando usuario con guid: {guid}");
            
            var userEntityExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid.ToLower() == guid.ToLower());
            if (userEntityExistente == null)
            {
                _logger.LogWarning($"Usuario no encontrado con guid: {guid}");
                return null;
            }

            if (userRequest.Username != userEntityExistente.Username && await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un usuario con el nombre: {userRequest.Username}");
                throw new UserExistException($"Ya existe un usuario con el nombre: {userRequest.Username}");
            }

            userEntityExistente.Username = userRequest.Username;
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
            return UserMapper.ToResponseFromEntity(userEntityExistente);
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
            return UserMapper.ToResponseFromEntity(userEntity);
        }
    }
}
