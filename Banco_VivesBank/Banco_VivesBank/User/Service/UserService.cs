using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using Role = Banco_VivesBank.User.Models.Role;

namespace Banco_VivesBank.User.Service
{
 public class UserService : IUserService 
{    
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IMemoryCache _memoryCache; 
    private readonly IDatabase _database;
    private const string CacheKeyPrefix = "User:"; 

    public UserService(
        GeneralDbContext context, 
        ILogger<UserService> logger, 
        IConnectionMultiplexer redis, 
        IMemoryCache memoryCache) 
    {
        _context = context;
        _logger = logger;
        _redis = redis;
        _memoryCache = memoryCache; 
        _database = _redis.GetDatabase();  
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los usuarios");
        var userEntityList = await _context.Usuarios.ToListAsync();
        return UserMapper.ToResponseListFromEntityList(userEntityList);
    }

    public async Task<UserResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando usuario con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
        {
            _logger.LogInformation("Usuario obtenido desde la memoria caché");
            return UserMapper.ToResponseFromModel(cachedUser);
        }
        
        // Intentar obtener desde la caché de Redis
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Usuario obtenido desde Redis");
            return JsonSerializer.Deserialize<UserResponse>(redisCache);
        }

        // Consultar la base de datos
        var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
        if (userEntity != null)
        {
            _logger.LogInformation($"Usuario encontrado con guid: {guid}");

            // Mapear entidad a modelo y respuesta
            var userResponse = UserMapper.ToResponseFromEntity(userEntity);
            var userModel = UserMapper.ToModelFromEntity(userEntity);

            // Guardar en la memoria caché
            _memoryCache.Set(cacheKey, userModel, TimeSpan.FromMinutes(30));

            // Guardar en Redis como JSON
            var redisValue = JsonSerializer.Serialize(userResponse);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return userResponse;
        }

        _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
        return null;
    }

        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"Buscando usuario con nombre de usuario: {username}");
            
            /*
            var cacheKey = CacheKeyPrefix + username;

            if (_memoryCache.TryGetValue(cacheKey, out Models.User? cachedUser))
            {
                _logger.LogInformation("Usuario obtenido desde cache");
                return cachedUser;
            }
            */

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (userEntity != null)
            {
                /*cachedUser = UserMapper.ToModelFromEntity(userEntity);
                _memoryCache.Set(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
                return cachedUser;*/
                
                _logger.LogInformation($"Usuario encontrado con nombre de usuario: {username}");
                return UserMapper.ToResponseFromEntity(userEntity);
            }

            _logger.LogInformation($"Usuario no encontrado con nombre de usuario: {username}");
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

            var userModel = UserMapper.ToModelFromRequest(userRequest);
            _context.Usuarios.Add(UserMapper.ToEntityFromModel(userModel));
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + userEntity.Id;
            _memoryCache.Set(cacheKey, UserMapper.ToModelFromEntity(userEntity), TimeSpan.FromMinutes(30));
            */

            _logger.LogInformation("Usuario creado con éxito");
            return UserMapper.ToResponseFromModel(userModel);
        }


        public async Task<UserResponse?> UpdateAsync(string guid, UserRequest userRequest)
        {
            _logger.LogInformation($"Actualizando usuario con guid: {guid}");
            
            var userEntityExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
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

        public async Task<UserResponse?> DeleteByGuidAsync(string guid)
        {
            _logger.LogInformation($"Borrando Usuario con guid: {guid}");
            
            var userExistenteEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
            if (userExistenteEntity == null)
            {
                _logger.LogWarning($"Usuario no encontrado con guid: {guid}");
                return null;
            }

            userExistenteEntity.IsDeleted = true;
            userExistenteEntity.UpdatedAt = DateTime.UtcNow;

            _context.Usuarios.Update(userExistenteEntity);
            await _context.SaveChangesAsync();

            /*
            var cacheKey = CacheKeyPrefix + id;
            _memoryCache.Remove(cacheKey);
            */

            _logger.LogInformation($"Usuario borrado (desactivado) con id: {guid}");
            return UserMapper.ToResponseFromEntity(userExistenteEntity);
        }

        public async Task<Models.User?> GetUserModelByGuid(string guid)
        {
            _logger.LogInformation($"Buscando usuario con guid: {guid}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con guid: {guid}");
                return UserMapper.ToModelFromEntity(userEntity);
            }

            _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
            return null;
        }
        
        public async Task<Models.User?> GetUserModelById(long id)
        {
            _logger.LogInformation($"Buscando usuario con id: {id}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con id: {id}");
                return UserMapper.ToModelFromEntity(userEntity);
            }

            _logger.LogInformation($"Usuario no encontrado con id: {id}");
            return null;
        }
    }
}
