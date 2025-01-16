using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vives_Bank_Net.Rest.User.Database;
using Vives_Bank_Net.Rest.User.Dtos;
using Vives_Bank_Net.Rest.User.Exceptions;
using Vives_Bank_Net.Rest.User.Mapper;

namespace Vives_Bank_Net.Rest.User.Service
{
    public class UserService : IUserService 
    {    
        private const string CacheKeyPrefix = "User_";
        private readonly UserDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly UserMapper _userMapper;
        
        public UserService(UserDbContext context, ILogger<UserService> logger, IMemoryCache memoryCache, UserMapper userMapper)
        {
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
            _userMapper = userMapper;
        }
        
        public async Task<List<User>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los usuarios");
            try
            {
                var userEntities = await _context.User.ToListAsync();
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

        public async Task<User?> GetByIdAsync(string id)
        {
            _logger.LogInformation($"Buscando Usuario con id: {id}");
            var cacheKey = CacheKeyPrefix + id;

            if (_memoryCache.TryGetValue(cacheKey, out User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }

            if (!long.TryParse(id, out long userId))
            {
                _logger.LogWarning($"Id con formato inválido: {id}");
                return null;
            }

            var userEntity = await _context.User.FindAsync(userId);
            if (userEntity != null)
            {
                cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;
            }

            return null;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"Buscando Usuario con username: {username}");
            var cacheKey = CacheKeyPrefix + username;

            if (_memoryCache.TryGetValue(cacheKey, out User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }

            var userEntity = await _context.User.FirstOrDefaultAsync(u => u.UserName == username);
            if (userEntity != null)
            {
                cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;
            }

            return null;
        }

        public async Task<UserResponse> CreateAsync(UserEntity userEntity)
        {
            _logger.LogInformation("Creando Usuario");

            if (await _context.User.AnyAsync(u => u.UserName.ToLower() == userEntity.UserName.ToLower()))
            {
                _logger.LogWarning($"Ya existe un Usuario con el nombre: {userEntity.UserName}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userEntity.UserName}");
            }

            _context.User.Add(userEntity);
            await _context.SaveChangesAsync();

            var cacheKey = CacheKeyPrefix + userEntity.Id;
            _memoryCache.Set(cacheKey, UserMapper.ToModelFromEntity(userEntity), TimeSpan.FromMinutes(30));
    
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

            var userExistente = await _context.User.FindAsync(userId);
            if (userExistente == null)
            {
                _logger.LogWarning($"Usuario no encontrado con id: {id}");
                return null;
            }

            if (userRequest.Username != userExistente.UserName && await _context.User.AnyAsync(u => u.UserName.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un Usuario con el nombre: {userRequest.Username}");
                throw new UserExistByUsername($"Ya existe un Usuario con el nombre: {userRequest.Username}");
            }

            userExistente.UserName = userRequest.Username;
            userExistente.PasswordHash = userRequest.PasswordHash;
            userExistente.UpdatedAt = DateTime.UtcNow;
            userExistente.IsDeleted = userRequest.IsDeleted;

            _context.User.Update(userExistente);
            await _context.SaveChangesAsync();
            
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
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

            var userEntity = await _context.User.FindAsync(userId);
            if (userEntity == null)
            {
                _logger.LogWarning($"Usuario no encontrado con id: {id}");
                return null;
            }

            userEntity.IsDeleted = true;
            userEntity.UpdatedAt = DateTime.UtcNow;

            _context.User.Update(userEntity);
            await _context.SaveChangesAsync();

            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);

            _logger.LogInformation($"Usuario borrado (desactivado) con id: {id}");
            return UserMapper.ToUserResponseFromEntity(userEntity);
        }
    }
}
