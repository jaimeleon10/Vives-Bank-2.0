using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Storage.Ftp.Service;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Images.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using Role = Banco_VivesBank.User.Models.Role;

namespace Banco_VivesBank.Cliente.Services;

public class ClienteService : IClienteService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly ILogger<ClienteService> _logger;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFtpService _ftpService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _redisDatabase;
    private const string CacheKeyPrefix = "Cliente:";

    public ClienteService(GeneralDbContext context, ILogger<ClienteService> logger, IUserService userService, IFileStorageService storageService, IMemoryCache memoryCache, IConnectionMultiplexer redis, IFtpService ftpService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
        _fileStorageService = storageService;
        _memoryCache = memoryCache;
        _redis = redis;
        _redisDatabase = _redis.GetDatabase();
        _ftpService = ftpService;
    }

    public async Task<PageResponse<ClienteResponse>> GetAllPagedAsync(string? nombre, string? apellidos, string? dni, PageRequest page)
    {
        _logger.LogInformation("Obteniendo todos los clientes paginados y filtrados");
        int pageNumber = page.PageNumber >= 0 ? page.PageNumber : 0;
        int pageSize = page.PageSize > 0 ? page.PageSize : 10;

        var query = _context.Clientes.Include(c => c.User).AsQueryable();

        query = page.SortBy.ToLower() switch
        {
            "nombre" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Nombre) 
                : query.OrderByDescending(c => c.Nombre),
            "apellidos" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Apellidos) 
                : query.OrderByDescending(c => c.Apellidos),
            "dni" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Dni) 
                : query.OrderByDescending(c => c.Dni),
            "id" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Id) 
                : query.OrderByDescending(c => c.Id),
            _ => throw new InvalidOperationException($"La propiedad {page.SortBy} no es válida para ordenamiento.")
        };
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            _logger.LogInformation($"Filtrando clientes por nombre {nombre}");;
            query = query.Where(c => c.Nombre.ToLower().Contains(nombre.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(apellidos))
        {  
           _logger.LogInformation($"Filtrando clientes por apellidos {apellidos}");
            query = query.Where(c => c.Apellidos.ToLower().Contains(apellidos.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(dni))
        {
            _logger.LogInformation($"Filtrando clientes por dni {dni}");
            query = query.Where(c => c.Dni.ToLower().Contains(dni.ToLower()));
        }

        query = query.OrderBy(c => c.Id);

        var totalElements = await query.CountAsync();
        
        var content = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        
        var totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
        
        var contentResponse = content.Select(ClienteMapper.ToResponseFromEntity).ToList();
        
        return new PageResponse<ClienteResponse>
        {
            Content = contentResponse,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalElements = totalElements,
            TotalPages = totalPages,
            Empty = !content.Any(),
            First = pageNumber == 0,
            Last = pageNumber == totalPages - 1,
            SortBy = page.SortBy,
            Direction = page.Direction
        };
    }

    public async Task<ClienteResponse?> GetByGuidAsync(string guid)
    {
		_logger.LogInformation($"Buscando cliente con guid: {guid}");

        var cacheKey = CacheKeyPrefix + guid;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Cliente? cachedCliente))
        {
            _logger.LogInformation("Cliente obtenido desde cache en memoria");
            return cachedCliente!.ToResponseFromModel();
        }
        
        var redisCacheCliente = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheCliente.IsNullOrEmpty)
        {
            _logger.LogInformation("Cliente obtenido desde cache en Redis");
            var clienteFromRedis = JsonSerializer.Deserialize<Models.Cliente>(redisCacheCliente!);
            if (clienteFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar usuario desde Redis");
                throw new Exception("Error al deserializar usuario desde Redis");
            }
            
            _memoryCache.Set(cacheKey, clienteFromRedis, TimeSpan.FromMinutes(30));
            return clienteFromRedis.ToResponseFromModel();
        }
        
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            var clienteResponse = clienteEntity.ToResponseFromEntity();
            var clienteModel = clienteEntity.ToModelFromEntity();

            _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));

            var redisValue = JsonSerializer.Serialize(clienteResponse);
            await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return clienteResponse;
        }
        
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    public async Task<ClienteResponse> CreateAsync(User.Models.User userAuth, ClienteRequest clienteRequest)
    {
        _logger.LogInformation("Creando cliente");
        
        ValidateDniExistente(clienteRequest.Dni);
        ValidateEmailExistente(clienteRequest.Email);
        ValidateTelefonoExistente(clienteRequest.Telefono);
        
        _logger.LogInformation($"Validando que el usuario con guid {userAuth.Guid} no sea ya un cliente");
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteEntityExistente != null)
        {
            _logger.LogWarning($"El usuario con guid {userAuth.Guid} ya es un cliente");
            throw new ClienteExistsException($"El usuario con guid {userAuth.Guid} ya es un cliente");
        }
        
        _logger.LogInformation($"Actualizando rol a cliente del usuario con guid {userAuth.Guid}");
        userAuth.Role = Role.Cliente;
        _context.Usuarios.Update(userAuth.ToEntityFromModel());
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Guardando cliente en base de datos");
        var clienteModel = clienteRequest.ToModelFromRequest(userAuth);
        var clienteEntity = clienteModel.ToEntityFromModel();
        _context.Clientes.Add(clienteEntity); 
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Guardando cliente con guid {clienteModel.Guid} en caché");
        var cacheKey = CacheKeyPrefix + clienteModel.Guid;
        var serializedCliente = JsonSerializer.Serialize(clienteModel);
        _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedCliente, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("Cliente creado con éxito");
        return clienteModel.ToResponseFromModel();
    }

    public async Task<ClienteResponse?> UpdateAsync(string guid, ClienteRequestUpdate clienteRequest){
        _logger.LogInformation($"Actualizando cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        if (clienteEntityExistente.Dni.ToLower() != clienteRequest.Dni.ToLower())
        {
            ValidateDniExistente(clienteRequest.Dni);
        }
        
        if (clienteEntityExistente.Email.ToLower() != clienteRequest.Email.ToLower())
        {
            ValidateEmailExistente(clienteRequest.Email);
        }
        
        if (clienteEntityExistente.Telefono != clienteRequest.Telefono)
        {
            ValidateTelefonoExistente(clienteRequest.Telefono);
        }

        clienteEntityExistente.Dni = clienteRequest.Dni;
        clienteEntityExistente.Nombre = clienteRequest.Nombre;
        clienteEntityExistente.Apellidos = clienteRequest.Apellidos;
        clienteEntityExistente.Direccion = new Direccion
        {
            Calle = clienteRequest.Calle,
            Numero = clienteRequest.Numero,
            CodigoPostal = clienteRequest.CodigoPostal,
            Piso = clienteRequest.Piso,
            Letra = clienteRequest.Letra
        };
        clienteEntityExistente.Email = clienteRequest.Email;
        clienteEntityExistente.Telefono = clienteRequest.Telefono;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        clienteEntityExistente.IsDeleted = clienteRequest.IsDeleted;
        
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteEntityExistente.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        var clienteModel = clienteEntityExistente.ToModelFromEntity();
        var redisValue = JsonSerializer.Serialize(clienteModel);
        await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

        _logger.LogInformation($"Cliente actualizado con guid: {guid}");
        return clienteEntityExistente.ToResponseFromEntity();
    }

    public async Task<ClienteResponse?> DeleteByGuidAsync(string guid) 
    {
        _logger.LogInformation($"Borrando cliente con guid: {guid}");
        
        var clienteExistenteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteExistenteEntity == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        clienteExistenteEntity.IsDeleted = true;
        clienteExistenteEntity.UpdatedAt = DateTime.UtcNow;

        _context.Clientes.Update(clienteExistenteEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteExistenteEntity.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {guid}");
        return clienteExistenteEntity.ToResponseFromEntity();
    }

    public async Task<string> DerechoAlOlvido(string userGuid)
    {
        _logger.LogInformation($"Borrando cliente del usuario con guid: {userGuid}");
        
        var user = await _userService.GetUserModelByGuidAsync(userGuid);
        if (user == null)
        {
            _logger.LogInformation($"Usuario no encontrado con guid: {userGuid}");
            throw new UserNotFoundException($"Usuario no encontrado con guid: {userGuid}");
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
        
        cliente = await DeleteData(cliente!);
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + cliente.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);

        _logger.LogInformation($"Datos del cliente eliminados de la base de datos");
        return "Datos del cliente eliminados de la base de datos";
    }
    
    private async Task<ClienteEntity> DeleteData(ClienteEntity entityCliente)
    {
        entityCliente.Dni = entityCliente.Nombre = entityCliente.Apellidos = entityCliente.Email = entityCliente.Telefono = string.Empty;
        entityCliente.Direccion = new Direccion
        {
            Calle = string.Empty, Numero = string.Empty, CodigoPostal = string.Empty, Piso = string.Empty, Letra = string.Empty
        };
        await _fileStorageService.DeleteFileAsync(entityCliente.FotoPerfil);
        await _fileStorageService.DeleteFileAsync(entityCliente.FotoDni);
        entityCliente.FotoPerfil = entityCliente.FotoDni = string.Empty;
        entityCliente.IsDeleted = true;
        entityCliente.UpdatedAt = DateTime.UtcNow;
        return entityCliente;
    }
    
    private void ValidateDniExistente(string dni)
    {
        _logger.LogInformation("Validando Dni");
        if (_context.Clientes.Any(c => c.Dni.ToLower() == dni.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el DNI: {dni}");
            throw new ClienteExistsException($"Ya existe un cliente con el DNI: {dni}");
        }
    }
    
    private void ValidateEmailExistente(string email)
    {
        _logger.LogInformation("Validando email");
        if(_context.Clientes.Any(c => c.Email.ToLower() == email.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el email: {email}");
            throw new ClienteExistsException($"Ya existe un cliente con el email: {email}");
        }
    }
    
    private void ValidateTelefonoExistente(string telefono)
    {
        _logger.LogInformation("Validando teléfono");
        if(_context.Clientes.Any(c => c.Telefono == telefono))
        {
            _logger.LogInformation($"Ya existe un cliente con el teléfono: {telefono}");
            throw new ClienteExistsException($"Ya existe un cliente con el teléfono: {telefono}");
        }
    }
    
    public async Task<ClienteResponse?> UpdateFotoPerfil(string guid, IFormFile fotoPerfil)
    {
        _logger.LogInformation($"Actualizando foto de perfil del cliente con guid: {guid}");

        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        var fotoAnterior = clienteEntityExistente.FotoPerfil;

        string nuevaFoto = null;
        try
        {
            nuevaFoto = await _fileStorageService.SaveFileAsync(fotoPerfil);
        }
        catch (FileStorageException ex)
        {
            _logger.LogError($"Error al guardar la nueva foto de perfil: {ex.Message}");
            throw new FileStorageException($"Error al guardar la foto: {ex.Message}");
        }

        if (nuevaFoto != null)
        {
            if (!string.IsNullOrEmpty(fotoAnterior) && fotoAnterior != "https://example.com/fotoPerfil.jpg")
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(fotoAnterior);
                }
                catch (FileStorageNotFoundException)
                {
                    _logger.LogInformation($"Archivo no encontrado, omitiendo la eliminación: {fotoAnterior}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al intentar eliminar la foto anterior: {ex.Message}");
                }
            }

            clienteEntityExistente.FotoPerfil = nuevaFoto;
            clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
            _context.Clientes.Update(clienteEntityExistente);
            await _context.SaveChangesAsync();

            return clienteEntityExistente.ToResponseFromEntity();
        }
        else
        {
            throw new FileStorageException("No se pudo guardar la foto de perfil.");
        }
    }

    public async Task<ClienteResponse?> UpdateFotoDni(string guid, IFormFile fotoDni)
    {
        _logger.LogInformation($"Actualizando foto del DNI del cliente con guid: {guid}");

        var clienteEntityExistente = await _context.Clientes.Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Guid == guid);

        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        string fileExtension = Path.GetExtension(fotoDni.FileName);
        string fileName = $"{clienteEntityExistente.Dni}{fileExtension}";
        string uploadPath = $"data/{fileName}";

        _logger.LogInformation($"Ruta final del archivo para subir: {uploadPath}");

        try
        {
            using (var stream = fotoDni.OpenReadStream())
            {
                await _ftpService.UploadFileAsync(stream, uploadPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al subir la nueva foto al FTP: {ex.Message}");
            throw new InvalidOperationException("Error al subir la nueva foto al servidor FTP.", ex);
        }

        if (!string.IsNullOrEmpty(clienteEntityExistente.FotoDni) &&
            clienteEntityExistente.FotoDni != "https://example.com/fotoDni.jpg")
        {
            try
            {
                await _ftpService.DeleteFileAsync(clienteEntityExistente.FotoDni);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al eliminar la foto anterior del FTP: {ex.Message}");
            }
        }

        clienteEntityExistente.FotoDni = uploadPath;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;

        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Foto del DNI del cliente con guid {guid} actualizada correctamente.");
        return clienteEntityExistente.ToResponseFromEntity();
    }
    
    public async Task<Stream> GetFotoDniAsync(string guid)
    {
        var clienteEntityExistente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Guid == guid);

        if (clienteEntityExistente == null || string.IsNullOrEmpty(clienteEntityExistente.FotoDni))
            return null;

        try
        {
            var localTempFile = Path.GetTempFileName();
            await _ftpService.DownloadFileAsync(clienteEntityExistente.FotoDni, localTempFile);

            return new FileStream(localTempFile, FileMode.Open, FileAccess.Read, 
                FileShare.Read, 4096, FileOptions.DeleteOnClose);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar foto DNI: {ex.Message}");
            throw;
        }
    }
    
    public async Task<IEnumerable<Models.Cliente>> GetAllForStorage()
    {
        _logger.LogInformation("Buscando todos los clientes en la base de datos");
        var clientes = await _context.Clientes.ToListAsync();
        return clientes.Select(c => ClienteMapper.ToModelFromEntity(c));
    }

    public async Task<Models.Cliente?> GetClienteModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cliente con guid: {guid}");
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return clienteEntity.ToModelFromEntity();
        }
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    public async Task<Models.Cliente?> GetClienteModelById(long id)
    {
        _logger.LogInformation($"Buscando Cliente con id: {id}");
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(t => t.Id == id);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con id: {id}");
            return clienteEntity.ToModelFromEntity();  
        }

        _logger.LogInformation($"Cliente no encontrado con id: {id}");
        return null;
    }
}
