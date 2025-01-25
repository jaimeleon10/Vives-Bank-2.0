using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Banco_VivesBank.Producto.Base.Services;

public class BaseService : IBaseService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly ILogger<BaseService> _logger;
    private readonly IMemoryCache _memoryCache; 
    private readonly IDatabase _database;
    private const string CacheKeyPrefix = "Producto:"; 
    
    public BaseService(GeneralDbContext context, ILogger<BaseService> logger, IConnectionMultiplexer redis, 
        IMemoryCache memoryCache)
    {
        _context = context;
        _logger = logger;
        _redis = redis;
        _memoryCache = memoryCache; 
        _database = _redis.GetDatabase();  
    }
    
    public async Task<IEnumerable<BaseResponse>> GetAllAsync()
    {
        _logger.LogDebug("Obteniendo todos los productos");
        var baseEntityList = await _context.ProductoBase.ToListAsync();
        return baseEntityList.ToResponseListFromEntityList();
    }

    public async Task<PageResponse<BaseResponse>> GetAllPagedAsync(PageRequest page)
    {
        _logger.LogInformation("Obteniendo todos los productos paginados y filtrados");
        int pageNumber = page.PageNumber >= 0 ? page.PageNumber : 0;
        int pageSize = page.PageSize > 0 ? page.PageSize : 10;

        var query = _context.ProductoBase.AsQueryable();

        query = page.SortBy.ToLower() switch
        {
            "id" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Id) 
                : query.OrderByDescending(c => c.Id),
            _ => throw new InvalidOperationException($"La propiedad {page.SortBy} no es válida para ordenamiento.")
        };

        query = query.OrderBy(c => c.Id);

        var totalElements = await query.CountAsync();
        
        var content = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        
        var totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
        
        var contentResponse = content.Select(BaseMapper.ToResponseFromEntity).ToList();
        
        return new PageResponse<BaseResponse>
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

    public async Task<BaseResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogDebug($"Obteniendo el producto con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Models.Base? cacheBase))
        {
            _logger.LogInformation("Producto obtenido desde la memoria caché");
            return cacheBase.ToResponseFromModel();
        }
        
        // Intentar obtener desde la caché de Redis
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Producto obtenido desde Redis");
            var baseResponse = JsonSerializer.Deserialize<BaseResponse>(redisCache);
            if (baseResponse != null)
            {
                _memoryCache.Set(cacheKey, baseResponse, TimeSpan.FromMinutes(30));
            }

            return baseResponse; 
        }

        // Consultar la base de datos
        var baseEntity = await _context.ProductoBase.FirstOrDefaultAsync(u => u.Guid == guid);
        if (baseEntity != null)
        {
            _logger.LogInformation($"Producto encontrado con guid: {guid}");

            // Mapear entidad a modelo y respuesta
            var baseResponse = baseEntity.ToResponseFromEntity();
            var baseModel = baseEntity.ToModelFromEntity();

            // Guardar en la memoria caché
            _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));

            // Guardar en Redis como JSON
            var redisValue = JsonSerializer.Serialize(cacheKey);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return baseResponse;
        }

        _logger.LogInformation($"Producto no encontrado con guid: {guid}");
        return null;
    }

    public async Task<BaseResponse?> GetByTipoAsync(string tipo)
    {
        _logger.LogInformation($"Buscando producto por tipo: {tipo}");

        var cacheKey = CacheKeyPrefix + tipo;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Models.Base? cacheBase))
        {
            _logger.LogInformation("Producto obtenido desde la memoria caché");
            return cacheBase.ToResponseFromModel();
        }
        
        // Intentar obtener desde la caché de Redis
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Producto obtenido desde Redis");
            var baseResponse = JsonSerializer.Deserialize<BaseResponse>(redisCache);
            if (baseResponse != null)
            {
                _memoryCache.Set(cacheKey, baseResponse, TimeSpan.FromMinutes(30));
            }

            return baseResponse; 
        }
        
        var baseEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.TipoProducto.ToLower() == tipo.ToLower());

        if (baseEntity != null)
        {
            _logger.LogInformation($"Producto encontrado con tipo: {tipo}");

            // Mapear entidad a modelo y respuesta
            var baseResponse = baseEntity.ToResponseFromEntity();
            var baseModel = baseEntity.ToModelFromEntity();

            // Guardar en la memoria caché
            _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));

            // Guardar en Redis como JSON
            var redisValue = JsonSerializer.Serialize(cacheKey);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return baseResponse;
        }

        _logger.LogInformation($"Producto no encontrado con tipo: {tipo}");
        return null;
    }

    public async Task<BaseResponse> CreateAsync(BaseRequest baseRequest)
    {
        _logger.LogInformation("Creando un nuevo producto base");

        if (await _context.ProductoBase.AnyAsync(b => b.Nombre.ToLower() == baseRequest.Nombre.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el nombre: {baseRequest.Nombre}");
            throw new BaseExistByNameException($"Ya existe un producto con el nombre: {baseRequest.Nombre}");
        }
        
        if (await _context.ProductoBase.AnyAsync(b => b.TipoProducto.ToLower() == baseRequest.TipoProducto.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el tipo: {baseRequest.TipoProducto}");
            throw new BaseDuplicateException($"Ya existe un producto con el tipo: {baseRequest.TipoProducto}");
        }
        
        var baseModel = baseRequest.ToModelFromRequest();
        var baseEntity = baseModel.ToEntityFromModel();
        _context.ProductoBase.Add(baseEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + baseModel.Guid;
        
        var serializedUser = JsonSerializer.Serialize(cacheKey);
        _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));
        await _database.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));

        
        _logger.LogInformation($"Producto creado correctamente: {baseModel}");
        return baseModel.ToResponseFromModel();

    }

    public async Task<BaseResponse?> UpdateAsync(string guid, BaseUpdateDto baseUpdate)
    {
        _logger.LogInformation($"Actualizando producto con guid: {guid}");
        
        var baseEntityExistente = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (baseEntityExistente == null)
        {
            _logger.LogWarning($"Producto con guid: {guid} no encontrado");
            throw new BaseNotExistException($"Producto con guid: {guid} no encontrado");
        }
    
        _logger.LogInformation("Validando nombre único");
        if (baseUpdate.Nombre != baseEntityExistente.Nombre && await _context.ProductoBase.AnyAsync(b => b.Nombre.ToLower() == baseUpdate.Nombre.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el nombre: {baseUpdate.Nombre}");
            throw new BaseExistByNameException($"Ya existe un producto con el nombre: {baseUpdate.Nombre}");
        }
        
        _logger.LogInformation("Actualizando producto");
        baseEntityExistente.Nombre = baseUpdate.Nombre;
        baseEntityExistente.Descripcion = baseUpdate.Descripcion;
        baseEntityExistente.Tae = baseUpdate.Tae;
        baseEntityExistente.UpdatedAt = DateTime.UtcNow;

        _context.ProductoBase.Update(baseEntityExistente);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + baseEntityExistente.ToModelFromEntity().Guid;
        
        _memoryCache.Remove(cacheKey);  
        await _database.KeyDeleteAsync(cacheKey);

        var baseModel = baseEntityExistente.ToModelFromEntity();
        
        var serializedUser = JsonSerializer.Serialize(cacheKey);
        _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));
        await _database.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));

        
        _logger.LogInformation($"Producto actualizado correctamente: {baseEntityExistente}");
        return baseEntityExistente.ToResponseFromEntity();

    }

    public async Task<BaseResponse?> DeleteAsync(string guid)
    {
        _logger.LogInformation($"Aplicando borrado logico a producto con guid: {guid}");
        var baseExistenteEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (baseExistenteEntity == null)
        {
            _logger.LogWarning($"Producto con id: {guid} no encontrado");
            return null;
        }

        _logger.LogInformation("Actualizando isDeleted a true");
        baseExistenteEntity.IsDeleted = true;
        baseExistenteEntity.UpdatedAt = DateTime.UtcNow;
        
        _context.ProductoBase.Update(baseExistenteEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + baseExistenteEntity.ToModelFromEntity().Guid;
        
        _memoryCache.Remove(cacheKey);
    
        await _database.KeyDeleteAsync(cacheKey);

        _logger.LogInformation($"Producto borrado logico correctamente con guid: {guid}");
        return baseExistenteEntity.ToResponseFromEntity();
    }
    
    public async Task<Models.Base?> GetBaseModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando producto con guid: {guid}");

        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (productoEntity != null)
        {
            _logger.LogInformation($"producto encontrado con guid: {guid}");
            return productoEntity.ToModelFromEntity();
        }

        _logger.LogInformation($"producto no encontrado con guid: {guid}");
        return null;
    }
        
    public async Task<Models.Base?> GetBaseModelById(long id)
    {
        _logger.LogInformation($"Buscando producto con id: {id}");

        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Id == id);
        if (productoEntity != null)
        {
            _logger.LogInformation($"producto encontrado con id: {id}");
            return productoEntity.ToModelFromEntity();
        }

        _logger.LogInformation($"producto no encontrado con id: {id}");
        return null;
    }
    
    //Acemos un GetAllByStorage que es un get all pero sin filtrado ni paginacion que devuelve en modelo
    public async Task<IEnumerable<Models.Base>> GetAllForStorage()
    {
        _logger.LogInformation("Obteniendo todos los productos");
        var productoEntityList = await _context.ProductoBase.ToListAsync();
        //hacemos un bucle para mapear uno a uno
        var baseModelList = new List<Models.Base>();
        foreach (var productoEntity in productoEntityList)
        {
            baseModelList.Add(BaseMapper.ToModelFromEntity(productoEntity));
        }
        return baseModelList;
    }

}