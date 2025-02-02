using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Mappers;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Banco_VivesBank.Producto.ProductoBase.Services;

/// <summary>
/// Servicio que gestiona las operaciones relacionadas con productos.
/// </summary>
public class ProductoService : IProductoService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly ILogger<ProductoService> _logger;
    private readonly IMemoryCache _memoryCache; 
    private readonly IDatabase _database;
    private const string CacheKeyPrefix = "Producto:"; 
    
    /// <summary>
    /// Constructor del servicio ProductoService.
    /// </summary>
    /// <param name="context">Contexto de base de datos.</param>
    /// <param name="logger">Instancia de logger para registrar eventos.</param>
    /// <param name="redis">Conexión a Redis.</param>
    /// <param name="memoryCache">Instancia de memoria caché.</param>
    public ProductoService(GeneralDbContext context, ILogger<ProductoService> logger, IConnectionMultiplexer redis, 
        IMemoryCache memoryCache)
    {
        _context = context;
        _logger = logger;
        _redis = redis;
        _memoryCache = memoryCache; 
        _database = _redis.GetDatabase();  
    }

    /// <summary>
    /// Obtiene todos los productos de forma paginada.
    /// </summary>
    /// <param name="page">Detalles de la paginación.</param>
    /// <returns>Una página con los productos y detalles de la paginación.</returns>
    public async Task<PageResponse<ProductoResponse>> GetAllPagedAsync(PageRequest page)
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
        
        var contentResponse = content.Select(ProductoMapper.ToResponseFromEntity).ToList();
        
        return new PageResponse<ProductoResponse>
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

    /// <summary>
    /// Obtiene un producto por su GUID.
    /// </summary>
    /// <param name="guid">El GUID del producto.</param>
    /// <returns>El producto correspondiente al GUID, o null si no se encuentra.</returns>
    public async Task<ProductoResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogDebug($"Obteniendo el producto con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out ProductoBase.Models.Producto? memoryCacheProducto))
        {
            _logger.LogInformation("Producto obtenido desde la memoria caché");
            return memoryCacheProducto!.ToResponseFromModel();
        }
        
        // Intentar obtener desde la caché de Redis
        var redisCacheProducto = await _database.StringGetAsync(cacheKey);
        if (!redisCacheProducto.IsNullOrEmpty)
        {
            _logger.LogInformation("Producto obtenido desde Redis");
            var productoFromRedis = JsonSerializer.Deserialize<Models.Producto>(redisCacheProducto!);
            if (productoFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar producto desde Redis");
                throw new Exception("Error al deserializar producto desde Redis");
            }
            _memoryCache.Set(cacheKey, productoFromRedis, TimeSpan.FromMinutes(30));
            return productoFromRedis.ToResponseFromModel(); 
        }

        // Consultar la base de datos
        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (productoEntity != null)
        {
            _logger.LogInformation($"Producto encontrado con guid: {guid}");

            // Guardar en la memoria caché
            _memoryCache.Set(cacheKey, productoEntity.ToModelFromEntity(), TimeSpan.FromMinutes(30));

            // Guardar en Redis como JSON
            var redisValue = JsonSerializer.Serialize(productoEntity.ToModelFromEntity());
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return productoEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Producto no encontrado con guid: {guid}");
        return null;
    }

    /// <summary>
    /// Obtiene un producto por su tipo.
    /// </summary>
    /// <param name="tipo">El tipo de producto.</param>
    /// <returns>El producto correspondiente al tipo, o null si no se encuentra.</returns>
    public async Task<ProductoResponse?> GetByTipoAsync(string tipo)
    {
        _logger.LogInformation($"Buscando producto por tipo: {tipo}");
        
        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.TipoProducto.ToLower() == tipo.ToLower());

        if (productoEntity != null)
        {
            _logger.LogInformation($"Producto encontrado con tipo: {tipo}");
            return productoEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Producto no encontrado con tipo: {tipo}");
        return null;
    }

    /// <summary>
    /// Crea un nuevo producto base.
    /// </summary>
    /// <param name="productoRequest">Los detalles del producto a crear.</param>
    /// <returns>El producto creado.</returns>
    public async Task<ProductoResponse> CreateAsync(ProductoRequest productoRequest)
    {
        _logger.LogInformation("Creando un nuevo producto base");

        if (await _context.ProductoBase.AnyAsync(b => b.Nombre.ToLower() == productoRequest.Nombre.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el nombre: {productoRequest.Nombre}");
            throw new ProductoExistByNameException($"Ya existe un producto con el nombre: {productoRequest.Nombre}");
        }
        
        if (await _context.ProductoBase.AnyAsync(b => b.TipoProducto.ToLower() == productoRequest.TipoProducto.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el tipo: {productoRequest.TipoProducto}");
            throw new ProductoDuplicatedException($"Ya existe un producto con el tipo: {productoRequest.TipoProducto}");
        }
        
        var baseModel = productoRequest.ToModelFromRequest();
        var baseEntity = baseModel.ToEntityFromModel();
        _context.ProductoBase.Add(baseEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + baseModel.Guid;
        
        var serializedProducto = JsonSerializer.Serialize(baseModel);
        _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));
        await _database.StringSetAsync(cacheKey, serializedProducto, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation($"Producto creado correctamente: {baseModel}");
        return baseModel.ToResponseFromModel();
    }

    /// <summary>
    /// Actualiza un producto existente por su GUID.
    /// </summary>
    /// <param name="guid">El GUID del producto a actualizar.</param>
    /// <param name="productoRequestUpdate">Los detalles para actualizar el producto.</param>
    /// <returns>El producto actualizado.</returns>
    public async Task<ProductoResponse?> UpdateAsync(string guid, ProductoRequestUpdate productoRequestUpdate)
    {
        _logger.LogInformation($"Actualizando producto con guid: {guid}");
        
        var baseEntityExistente = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (baseEntityExistente == null)
        {
            _logger.LogWarning($"Producto con guid: {guid} no encontrado");
            throw new ProductoNotExistException($"Producto con guid: {guid} no encontrado");
        }
    
        _logger.LogInformation("Validando nombre único");
        if (productoRequestUpdate.Nombre != baseEntityExistente.Nombre && await _context.ProductoBase.AnyAsync(b => b.Nombre.ToLower() == productoRequestUpdate.Nombre.ToLower()))
        {
            _logger.LogWarning($"Ya existe un producto con el nombre: {productoRequestUpdate.Nombre}");
            throw new ProductoExistByNameException($"Ya existe un producto con el nombre: {productoRequestUpdate.Nombre}");
        }
        
        _logger.LogInformation("Actualizando producto");
        baseEntityExistente.Nombre = productoRequestUpdate.Nombre;
        baseEntityExistente.Descripcion = productoRequestUpdate.Descripcion;
        baseEntityExistente.Tae = productoRequestUpdate.Tae;
        baseEntityExistente.UpdatedAt = DateTime.UtcNow;
        baseEntityExistente.IsDeleted = productoRequestUpdate.IsDeleted;

        _context.ProductoBase.Update(baseEntityExistente);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + baseEntityExistente.Guid;
        
        _memoryCache.Remove(cacheKey);  
        await _database.KeyDeleteAsync(cacheKey);

        var baseModel = baseEntityExistente.ToModelFromEntity();
        
        var serializedProduct = JsonSerializer.Serialize(baseModel);
        _memoryCache.Set(cacheKey, baseModel, TimeSpan.FromMinutes(30));
        await _database.StringSetAsync(cacheKey, serializedProduct, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation($"Producto actualizado correctamente");
        return baseEntityExistente.ToResponseFromEntity();
    }

    /// <summary>
    /// Elimina un producto de forma lógica (cambia el campo IsDeleted a true).
    /// </summary>
    /// <param name="guid">El GUID del producto a eliminar.</param>
    public async Task<ProductoResponse?> DeleteByGuidAsync(string guid)
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
        
        var cacheKey = CacheKeyPrefix + baseExistenteEntity.Guid;
        
        _memoryCache.Remove(cacheKey);
    
        await _database.KeyDeleteAsync(cacheKey);

        _logger.LogInformation($"Producto borrado logico correctamente con guid: {guid}");
        return baseExistenteEntity.ToResponseFromEntity();
    }
    
    /// <summary>
    /// Obtiene el modelo base de un producto por su GUID.
    /// </summary>
    /// <param name="guid">El GUID del producto.</param>
    /// <returns>El modelo base del producto correspondiente al GUID.</returns>
    public async Task<ProductoBase.Models.Producto?> GetBaseModelByGuid(string guid)
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
    
    /// <summary>
    /// Obtiene el modelo base de un producto por su ID.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <returns>El modelo base del producto correspondiente al ID.</returns>    
    public async Task<ProductoBase.Models.Producto?> GetBaseModelById(long id)
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
    
    /// <summary>
    /// Obtiene todos los productos sin paginación ni filtrado.
    /// </summary>
    /// <returns>Una lista con todos los productos.</returns>
    public async Task<IEnumerable<ProductoBase.Models.Producto>> GetAllForStorage()
    {
        _logger.LogInformation("Obteniendo todos los productos");
        var productoEntityList = await _context.ProductoBase.ToListAsync();
        //hacemos un bucle para mapear uno a uno
        var baseModelList = new List<ProductoBase.Models.Producto>();
        foreach (var productoEntity in productoEntityList)
        {
            baseModelList.Add(ProductoMapper.ToModelFromEntity(productoEntity));
        }
        return baseModelList;
    }
}