using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Storage.Files.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Banco_VivesBank.Cliente.Services;

public class ClienteService : IClienteService
{
    private readonly GeneralDbContext _context;
    private readonly ILogger<ClienteService> _logger;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _database;
    private const string CacheKeyPrefix = "Cliente:";

    public ClienteService(GeneralDbContext context, ILogger<ClienteService> logger, IUserService userService, IFileStorageService storageService, IMemoryCache memoryCache)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
        _fileStorageService = storageService;
        _memoryCache = memoryCache;
    }
    
    public async Task<IEnumerable<ClienteResponse>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los clientes");
		var clientesEntityList = await _context.Clientes.Include(c=> c.User).ToListAsync();
        var clienteResponseList = new List<ClienteResponse>();
        foreach (var clienteEntity in clientesEntityList)
        {
            var clienteResponse = ClienteMapper.ToResponseFromEntity(clienteEntity);
            clienteResponseList.Add(clienteResponse);
        }

        return clienteResponseList;
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
            return ClienteMapper.ToResponseFromModel(cachedCliente);
        }
        
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Cliente obtenido desde cache en Redis");
            return JsonSerializer.Deserialize<ClienteResponse>(redisCache);
        }
        
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            var clienteResponse = ClienteMapper.ToResponseFromEntity(clienteEntity);
            var clienteModel = ClienteMapper.ToModelFromEntity(clienteEntity);

            _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));

            var redisValue = JsonSerializer.Serialize(clienteResponse);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return clienteResponse;
        }
        
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    public async Task<ClienteResponse> CreateAsync(ClienteRequest clienteRequest)
    {
        _logger.LogInformation("Creando cliente");
        
        ValidateDniExistente(clienteRequest.Dni);
        ValidateEmailExistente(clienteRequest.Email);
        ValidateTelefonoExistente(clienteRequest.Telefono);
        
        var user = await _userService.GetUserModelByGuid(clienteRequest.UserGuid);
        if (user == null)
        {
            _logger.LogInformation($"Usuario no encontrado con guid: {clienteRequest.UserGuid}");
            throw new UserNotFoundException($"Usuario no encontrado con guid: {clienteRequest.UserGuid}");
        }
        
        var clienteModel = ClienteMapper.ToModelFromRequest(clienteRequest, user);
        _context.Clientes.Add(ClienteMapper.ToEntityFromModel(clienteModel)); 
        await _context.SaveChangesAsync();

        var cacheKey = CacheKeyPrefix + clienteModel.Dni;
        _memoryCache.Set(cacheKey, clienteModel);
        var redisValue = JsonSerializer.Serialize(ClienteMapper.ToResponseFromModel(clienteModel));
        await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

        _logger.LogInformation("Cliente creado con éxito");
        return ClienteMapper.ToResponseFromModel(clienteModel);
    }

    public async Task<ClienteResponse?> UpdateAsync(string guid, ClienteRequestUpdate clienteRequestUpdate){
        _logger.LogInformation($"Actualizando cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }
        if(!clienteRequestUpdate.HasAtLeastOneField())
        {
            _logger.LogInformation("No se ha modificado ningún campo");
            return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
        }

        if (clienteEntityExistente.Dni != clienteRequestUpdate.Dni && !string.IsNullOrEmpty(clienteRequestUpdate.Dni))
        {
            ValidateDniExistente(clienteRequestUpdate.Dni);
        }
        if (clienteEntityExistente.Email != clienteRequestUpdate.Email && !string.IsNullOrEmpty(clienteRequestUpdate.Email))
        {
            ValidateEmailExistente(clienteRequestUpdate.Email);
        }
        if (clienteEntityExistente.Telefono != clienteRequestUpdate.Telefono && !string.IsNullOrEmpty(clienteRequestUpdate.Telefono))
        {
            ValidateTelefonoExistente(clienteRequestUpdate.Telefono);
        }
        var clienteUpdated = ClienteMapper.ToModelFromRequestUpdate(clienteEntityExistente, clienteRequestUpdate);
        var clienteModel = ClienteMapper.ToModelFromEntity(clienteUpdated);
        
        _context.Clientes.Update(clienteUpdated);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteUpdated.Dni;
        _memoryCache.Remove(cacheKey);
        var redisValue = JsonSerializer.Serialize(ClienteMapper.ToResponseFromModel(clienteModel));
        await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

        _logger.LogInformation($"Cliente actualizado con guid: {guid}");
        return ClienteMapper.ToResponseFromEntity(clienteUpdated);
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

        var clienteToDelete = ClienteMapper.ToModelFromEntity(clienteExistenteEntity);

        var cacheKey = CacheKeyPrefix + clienteToDelete.Dni;
        _memoryCache.Remove(cacheKey);

        var redisValue = JsonSerializer.Serialize(ClienteMapper.ToResponseFromModel(clienteToDelete));
        await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {guid}");
        
        return ClienteMapper.ToResponseFromEntity(clienteExistenteEntity);
    }

    public async Task<string> DerechoAlOlvido(string userGuid)
    {
        _logger.LogInformation($"Borrando cliente del usuario con guid: {userGuid}");
        
        var user = await _userService.GetUserModelByGuid(userGuid);
        if (user == null)
        {
            _logger.LogInformation($"Usuario no encontrado con guid: {userGuid}");
            throw new UserNotFoundException($"Usuario no encontrado con guid: {userGuid}");
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
        
        cliente = DeleteData(cliente!);
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Datos del cliente eliminados de la base de datos");
        return "Datos del cliente eliminados de la base de datos";
    }
    
    private ClienteEntity DeleteData(ClienteEntity entityCliente)
    {
        entityCliente.Dni = "";
        entityCliente.Nombre = "";
        entityCliente.Apellidos = "";
        entityCliente.Direccion.Calle = "";
        entityCliente.Direccion.Numero = "";
        entityCliente.Direccion.CodigoPostal = "";
        entityCliente.Direccion.Piso = "";
        entityCliente.Direccion.Letra = "";
        entityCliente.Email = "";
        entityCliente.Telefono = "";
        _fileStorageService.DeleteFileAsync(entityCliente.FotoPerfil);
        _fileStorageService.DeleteFileAsync(entityCliente.FotoDni);
        entityCliente.FotoPerfil= "";
        entityCliente.FotoDni = "";
        entityCliente.IsDeleted = true;
        entityCliente.UpdatedAt = DateTime.Now;

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
        if (clienteEntityExistente.FotoPerfil != "https://example.com/fotoPerfil.jpg")
        {
            await _fileStorageService.DeleteFileAsync(fotoAnterior);
            
        }
        var nuevaFoto = await _fileStorageService.SaveFileAsync(fotoPerfil);
        clienteEntityExistente.FotoPerfil = nuevaFoto;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
    }

    public async Task<ClienteResponse?> UpdateFotoDni(string guid, IFormFile fotoDni)
    {
        _logger.LogInformation($"Actualizando foto del DNI del cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        var fotoAnterior = clienteEntityExistente.FotoDni;
        if (clienteEntityExistente.FotoDni != "https://example.com/fotoDni.jpg")
        {
            await _fileStorageService.DeleteFileAsync(fotoAnterior);
            
        }
        var nuevaFoto = await _fileStorageService.SaveFileAsync(fotoDni);
        clienteEntityExistente.FotoDni= nuevaFoto;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
    }

    public async Task<Models.Cliente?> GetClienteModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cliente con guid: {guid}");

        var cacheKey = CacheKeyPrefix + guid.ToLower();
        if (_memoryCache.TryGetValue(cacheKey, out Models.Cliente? cachedCliente))
        {
            _logger.LogInformation("Cliente obtenido desde cache en memoria");
            return cachedCliente;
        }

        var cachedClienteRedis = await _database.StringGetAsync(cacheKey);
        if (cachedClienteRedis.HasValue)
        {
            _logger.LogInformation("Cliente obtenido desde cache en Redis");
            var clienteFromRedis = JsonSerializer.Deserialize<Models.Cliente>(cachedClienteRedis);

            if (clienteFromRedis != null)
            {
                _memoryCache.Set(cacheKey, clienteFromRedis, TimeSpan.FromMinutes(30));
            }

            return clienteFromRedis;
        }

        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
            if (clienteEntity != null)
            {
                _logger.LogInformation($"Cliente encontrado con guid: {guid}");
                var clienteModel = ClienteMapper.ToModelFromEntity(clienteEntity);
                _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));
                var serializedCliente = JsonSerializer.Serialize(clienteModel);
                await _database.StringSetAsync(cacheKey, serializedCliente, TimeSpan.FromMinutes(30));
                return clienteModel;
            }

            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }
    

    public async Task<Models.Cliente?> GetClienteModelById(long id)
    {
        _logger.LogInformation($"Buscando Cliente con id: {id}");
        
        var cacheKey = CacheKeyPrefix + id;
        if (_memoryCache.TryGetValue(cacheKey, out Models.Cliente? cachedCliente))
        {
            _logger.LogInformation("Cliente obtenido desde cache en memoria");
            return cachedCliente;  
        }
        var cachedClienteRedis = await _database.StringGetAsync(cacheKey);
        if (cachedClienteRedis.HasValue)
        {
            _logger.LogInformation("Cliente obtenido desde cache en Redis");
            
            var clienteFromRedis = JsonSerializer.Deserialize<Models.Cliente>(cachedClienteRedis);
            if (clienteFromRedis != null)
            {
                _memoryCache.Set(cacheKey, clienteFromRedis, TimeSpan.FromMinutes(30));
            }
            return clienteFromRedis; 
        }

        var clienteEntity = await _context.Clientes.FirstOrDefaultAsync(t => t.Id == id);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con id: {id}");
            var clienteModel = ClienteMapper.ToModelFromEntity(clienteEntity);
            _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));
            var serializedCliente = JsonSerializer.Serialize(clienteModel);
            await _database.StringSetAsync(cacheKey, serializedCliente, TimeSpan.FromMinutes(30));
            return clienteModel;  
        }

        _logger.LogInformation($"Cliente no encontrado con id: {id}");
        return null;
    }
    
}
