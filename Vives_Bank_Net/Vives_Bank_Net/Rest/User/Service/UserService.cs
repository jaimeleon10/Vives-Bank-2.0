using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vives_Bank_Net.Rest.Database;
using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dto;
using Vives_Bank_Net.Rest.User.Exceptions;
using Vives_Bank_Net.Rest.User.Mapper;

namespace Vives_Bank_Net.Rest.User.Service
{
    public class UserService : IUserService 
    {    
        private const string CacheKeyPrefix = "User_";
        private readonly GeneralDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly UserMapper _userMapper;
        
        public UserService(GeneralDbContext context, ILogger<UserService> logger, UserMapper userMapper)
        {
            _context = context;
            _logger = logger;
            _userMapper = userMapper;
        }
        
        public async Task<List<Models.User>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los usuarios");
            try
            {
                var userEntities = await _context.Usuarios.ToListAsync();
                if (userEntities == null || !userEntities.Any())
                {
                    throw new UserNotFoundException("No se han encontrado usuarios");
                }
                _logger.LogInformation("Usuarios obtenidos con éxito");
                return userEntities.Select(UserMapper.ToModelFromEntity).ToList();
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

        public async Task<Models.User?> GetByIdAsync(string id)
        {
            _logger.LogInformation($"Buscando Usuario con id: {id}");
            var cacheKey = CacheKeyPrefix + id;

            /*if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }*/

            if (!long.TryParse(id, out long userId))
            {
                _logger.LogWarning($"Id con formato inválido: {id}");
                return null;
            }

            var userEntity = await _context.Usuarios.FindAsync(userId);
            if (userEntity != null)
            {
                /*cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;*/
                
                return UserMapper.ToModelFromEntity(userEntity);
            }

            return null;
        }

        public async Task<Models.User?> GetByUsernameAsync(string username)
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
                
                return UserMapper.ToModelFromEntity(userEntity);
            }

            return null;
        }

        public async Task<UserResponse> CreateAsync(UserEntity userEntity)
        {
            _logger.LogInformation("Creando Usuario");

            if (await _context.Usuarios.AnyAsync(u => u.UserName.ToLower() == userEntity.UserName.ToLower()))
            {
                _logger.LogWarning($"Ya existe un Usuario con el nombre: {userEntity.UserName}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userEntity.UserName}");
            }

            _context.Usuarios.Add(userEntity);
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + userEntity.Id;
            _memoryCache.Set(cacheKey, UserMapper.ToModelFromEntity(userEntity), TimeSpan.FromMinutes(30));
            */

            _logger.LogInformation("Usuario creado con éxito");
            return UserMapper.ToUserResponseFromEntity(userEntity);
        }


        public async Task<UserResponse?> UpdateAsync(string id, UserRequestDto userRequest)
        {
            _logger.LogInformation($"Actualizando Usuario con id: {id}");
            if (!long.TryParse(id, out var userId))
            {
                _logger.LogWarning($"Id con formato inválido: {id}");
                return null;
            }

            var userExistente = await _context.Usuarios.FindAsync(userId);
            if (userExistente == null)
            {
                _logger.LogWarning($"Usuario no encontrado con id: {id}");
                return null;
            }

            if (userRequest.Username != userExistente.UserName && await _context.Usuarios.AnyAsync(u => u.UserName.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un Usuario con el nombre: {userRequest.Username}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userRequest.Username}");
            }

            userExistente.UserName = userRequest.Username;
            userExistente.PasswordHash = userRequest.PasswordHash;
            userExistente.UpdatedAt = DateTime.Now;
            userExistente.IsDeleted = userRequest.IsDeleted;

            _context.Usuarios.Update(userExistente);
            await _context.SaveChangesAsync();
            
            /*
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
            */
            
            _logger.LogInformation($"Usuario actualizado con id: {id}");
            return UserMapper.ToUserResponseFromEntity(userExistente);
        }

        public async Task<UserResponse?> DeleteByIdAsync(string id)
        {
            _logger.LogInformation($"Borrando Usuario con id: {id}");
            if (!long.TryParse(id, out var userId))
            {
                _logger.LogWarning($"Id con formato inválido: {id}");
                return null;
            }

            var userEntity = await _context.Usuarios.FindAsync(userId);
            if (userEntity == null)
            {
                _logger.LogWarning($"Usuario no encontrado con id: {id}");
                return null;
            }

            userEntity.IsDeleted = true;
            userEntity.UpdatedAt = DateTime.Now;

            _context.Usuarios.Update(userEntity);
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
            */

            _logger.LogInformation($"Usuario borrado (desactivado) con id: {id}");
            return UserMapper.ToUserResponseFromEntity(userEntity);
        }
    }
}
