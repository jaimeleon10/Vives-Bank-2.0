using System.Security.Claims;
using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Database;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.Utils.Auth.Jwt;
using Banco_VivesBank.Utils.Pagination;
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
        private readonly IDatabase _redisDatabase;
        private const string CacheKeyPrefix = "User:";
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            GeneralDbContext context,
            ILogger<UserService> logger,
            IConnectionMultiplexer redis,
            IMemoryCache memoryCache, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
            _memoryCache = memoryCache;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _redisDatabase = _redis.GetDatabase();
        }
        
        public async Task<PageResponse<UserResponse>> GetAllAsync(string? username, Role? role, PageRequest pageRequest)
        {
            _logger.LogInformation("Buscando todos los usuarios en la base de datos");
            int pageNumber = pageRequest.PageNumber >= 0 ? pageRequest.PageNumber : 0;
            int pageSize = pageRequest.PageSize > 0 ? pageRequest.PageSize : 10;

            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(e => e.Username.ToLower().Contains(username.ToLower()));
            }

            if (role != null)
            {
                query = query.Where(e => e.Role == role);
            }


            if (!string.IsNullOrEmpty(pageRequest.SortBy))
            {
                query = pageRequest.Direction.ToUpper() == "ASC"
                    ? query.OrderBy(e => EF.Property<object>(e, pageRequest.SortBy))
                    : query.OrderByDescending(e => EF.Property<object>(e, pageRequest.SortBy));
            }

            var totalElements = await query.CountAsync();

            var content = await query
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalElements / pageSize);

            var pageResponse = new PageResponse<UserResponse>
            {
                Content = content.Select(UserMapper.ToResponseFromEntity).ToList(),
                TotalPages = totalPages,
                TotalElements = totalElements,
                PageSize = pageSize,
                PageNumber = pageNumber,
                Empty = !content.Any(),
                First = pageNumber == 0,
                Last = pageNumber == totalPages - 1,
                SortBy = pageRequest.SortBy,
                Direction = pageRequest.Direction
            };

            return pageResponse;
        }
        
        public async Task<IEnumerable<Models.User>> GetAllForStorage()
        {
            _logger.LogInformation("Buscando todos los usuarios en base de datos");
            var usersEntities = await _context.Usuarios.ToListAsync();
            return usersEntities.Select(UserMapper.ToModelFromEntity).ToList();
        }

        public async Task<UserResponse?> GetByGuidAsync(string guid)
        {
            _logger.LogInformation($"Buscando usuario con guid: {guid}");

            var cacheKey = CacheKeyPrefix + guid;

            // Intentar obtener desde la memoria caché
            if (_memoryCache.TryGetValue(cacheKey, out Models.User? memoryCacheUser))
            {
                _logger.LogInformation("Usuario obtenido desde la memoria caché");
                return memoryCacheUser!.ToResponseFromModel();
            }

            // Intentar obtener desde la caché de Redis
            var redisCacheUser = await _redisDatabase.StringGetAsync(cacheKey);
            if (!redisCacheUser.IsNullOrEmpty)
            {
                _logger.LogInformation("Usuario obtenido desde Redis");
                var userFromRedis = JsonSerializer.Deserialize<Models.User>(redisCacheUser!);
                if (userFromRedis == null)
                {
                    _logger.LogWarning("Error al deserializar usuario desde Redis");
                    throw new Exception("Error al deserializar usuario desde Redis");
                }

                _memoryCache.Set(cacheKey, userFromRedis, TimeSpan.FromMinutes(30));
                return userFromRedis.ToResponseFromModel();
            }

            // Consultar la base de datos
            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con guid: {guid}");

                // Mapear entidad a modelo y respuesta
                var userModel = userEntity.ToModelFromEntity();
                var userResponse = userModel.ToResponseFromModel();

                // Guardar en la memoria caché
                _memoryCache.Set(cacheKey, userModel, TimeSpan.FromMinutes(30));

                // Guardar en Redis como JSON
                var redisValue = JsonSerializer.Serialize(userModel);
                await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

                return userResponse;
            }

            _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
            return null;
        }

        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"Buscando usuario con nombre de usuario: {username}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con nombre de usuario: {username}");
                return userEntity.ToResponseFromEntity();
            }

            _logger.LogInformation($"Usuario no encontrado con nombre de usuario: {username}");
            return null;
        }
        
        public async Task<Models.User?> GetUserModelByGuidAsync(string guid)
        {
            _logger.LogInformation($"Buscando usuario con guid: {guid}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con guid: {guid}");
                return userEntity.ToModelFromEntity();
            }

            _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
            return null;
        }

        public async Task<Models.User?> GetUserModelByIdAsync(long id)
        {
            _logger.LogInformation($"Buscando usuario con id: {id}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con id: {id}");
                return userEntity.ToModelFromEntity();
            }

            _logger.LogInformation($"Usuario no encontrado con id: {id}");
            return null;
        }

        public async Task<UserResponse?> GetMeAsync(Models.User userAuth)
        {
            _logger.LogInformation($"Buscando usuario con guid: {userAuth.Guid}");

            var userEntity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == userAuth.Guid);
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con guid: {userAuth.Guid}");
                return userEntity.ToResponseFromEntity();
            }

            _logger.LogInformation($"Usuario no encontrado con id: {userAuth.Guid}");
            return null;
        }

        public async Task<UserResponse> CreateAsync(UserRequest userRequest)
        {
            _logger.LogInformation("Creando Usuario");

            if (await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == userRequest.Username.ToLower()))
            {
                _logger.LogWarning($"Ya existe un usuario con el nombre: {userRequest.Username} (en base de datos)");
                throw new UserExistException($"Ya existe un Usuario con el nombre: {userRequest.Username}");
            }
            
            _logger.LogInformation($"Verificando que las contraseñas coinciden");
            if (userRequest.Password != userRequest.PasswordConfirmation)
            {
                _logger.LogWarning("Las contraseñas no coinciden");
                throw new InvalidPasswordException("Las contraseñas no coinciden");
            }

            //Crear Usuario
            var userModel = userRequest.ToModelFromRequest();
            
            // Generar el hash con bcrypt
            userModel.Password = BCrypt.Net.BCrypt.HashPassword(userRequest.Password);
            
            var userEntity = userModel.ToEntityFromModel();
            _context.Usuarios.Add(userEntity);
            await _context.SaveChangesAsync();

            var cacheKey = CacheKeyPrefix + userModel.Guid;

            // Guardar el usuario 
            var serializedUser = JsonSerializer.Serialize(userModel);
            _memoryCache.Set(cacheKey, userModel, TimeSpan.FromMinutes(30));
            await _redisDatabase.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Usuario creado con éxito");
            return userModel.ToResponseFromModel();
        }

        public async Task<UserResponse> UpdatePasswordAsync(Models.User user, UpdatePasswordRequest updatePasswordRequest)
        {
            if (!BCrypt.Net.BCrypt.Verify(updatePasswordRequest.OldPassword, user.Password))
            {
                _logger.LogWarning("La actual no coincide con el campo antigua contraseña");
                throw new InvalidPasswordException("La contraseña actual no coincide con el campo antigua contraseña");
            }

            if (updatePasswordRequest.NewPassword != updatePasswordRequest.NewPasswordConfirmation)
            {
                _logger.LogWarning("La nueva contraseña no coincide con la confirmación");
                throw new InvalidPasswordException("La nueva contraseña no coincide con la confirmación");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.NewPassword);
            var userEntity = user.ToEntityFromModel();
            _context.Usuarios.Update(userEntity);
            await _context.SaveChangesAsync();
            
            return userEntity.ToResponseFromEntity();
        }

        public async Task<UserResponse?> UpdateAsync(string guid, UserRequestUpdate userRequestUpdate)
        {
            _logger.LogInformation($"Actualizando usuario con guid: {guid}");

            var userEntityExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Guid == guid);
            
            if (userEntityExistente == null)
            {
                _logger.LogWarning($"Usuario no encontrado con guid: {guid}");
                return null;
            }

            userEntityExistente.Role = Enum.Parse<Role>(userRequestUpdate.Role, true);
            userEntityExistente.UpdatedAt = DateTime.UtcNow;
            userEntityExistente.IsDeleted = userRequestUpdate.IsDeleted;

            _context.Usuarios.Update(userEntityExistente);
            await _context.SaveChangesAsync();

            var cacheKey = CacheKeyPrefix + userEntityExistente.Guid;
            _memoryCache.Remove(cacheKey);
            await _redisDatabase.KeyDeleteAsync(cacheKey);

            _memoryCache.Set(cacheKey, userEntityExistente.ToModelFromEntity(), TimeSpan.FromMinutes(30));
            var redisValue = JsonSerializer.Serialize(userEntityExistente.ToModelFromEntity());
            await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            _logger.LogInformation($"Usuario actualizado con guid: {guid}");
            return userEntityExistente.ToResponseFromEntity();
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
        
            _logger.LogInformation($"Desactivando cliente, cuentas y tarjetas pertenecientes al usuario con guid {userExistenteEntity.Guid}");
            var clienteExistenteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == guid);
            if (clienteExistenteEntity == null)
            {
                _logger.LogWarning($"Cliente no encontrado para el usuario con guid: {guid}");
                throw new ClienteNotFound($"Cliente no encontrado para el usuario con guid: {guid}");
            }

            clienteExistenteEntity.IsDeleted = true;
            clienteExistenteEntity.UpdatedAt = DateTime.UtcNow;
            _context.Clientes.Update(clienteExistenteEntity);
            
            var cuentas = await _context.Cuentas.Where(c => c.ClienteId == clienteExistenteEntity.Id).ToListAsync();
            foreach (var cuenta in cuentas)
            {
                cuenta.IsDeleted = true;
                cuenta.UpdatedAt = DateTime.UtcNow;
                _context.Cuentas.Update(cuenta);

                if (cuenta.TarjetaId == null) continue;
                var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuenta.TarjetaId);
                tarjetaExistente!.IsDeleted = true;
                tarjetaExistente!.UpdatedAt = DateTime.UtcNow;
            
                _context.Tarjetas.Update(tarjetaExistente);
            }
        
            _logger.LogInformation("Guardando todos los cambios en la base de datos");
            await _context.SaveChangesAsync();

            var cacheKey = CacheKeyPrefix + userExistenteEntity.Guid;

            // Eliminar de la cache en memoria
            _memoryCache.Remove(cacheKey);

            // Eliminar de Redis
            await _redisDatabase.KeyDeleteAsync(cacheKey);

            _logger.LogInformation($"Usuario borrado (desactivado) con id: {guid}");
            return userExistenteEntity.ToResponseFromEntity();
        }

        public string Authenticate(string username, string password)
        {
            _logger.LogInformation($"Buscando usuario para realizar login");
            var user = _context.Usuarios.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                _logger.LogWarning($"Credenciales inválidas para el usuario: {username}");
                throw new UnauthorizedAccessException($"Credenciales inválidas para el usuario: {username}");
            }
            
            _logger.LogInformation($"Usuario encontrado y verificado, generando Token");
            return _jwtService.GenerateToken(user.ToModelFromEntity());
        }
        
        public Models.User? GetAuthenticatedUser()
        {
            _logger.LogInformation("Buscando usuario autenticado");
            var username = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("No se ha encontrado el nombre de usuario en el contexto de seguridad");
                return null;
            }

            _logger.LogInformation($"Buscando usuario con nombre de usuario: {username}");
            var userEntity = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            if (userEntity != null)
            {
                _logger.LogInformation($"Usuario encontrado con nombre de usuario: {username}");
                return userEntity.ToModelFromEntity();
            }

            _logger.LogInformation($"Usuario no encontrado con nombre de usuario: {username}");
            return null;
        }
    }
}