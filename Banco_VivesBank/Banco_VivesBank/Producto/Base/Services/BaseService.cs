using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Producto.Base.Services;

public class BaseService : IBaseService
{
    //private const string CacheKeyPrefix = "Base_";
    private readonly GeneralDbContext _context;
    private readonly ILogger<BaseService> _logger;
    
    public BaseService(GeneralDbContext context, ILogger<BaseService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IEnumerable<BaseResponse>> GetAllAsync()
    {
        _logger.LogDebug("Obteniendo todos los productos");
        var baseEntityList = await _context.ProductoBase.ToListAsync();
        return BaseMapper.ToResponseListFromEntityList(baseEntityList);
    }

    public async Task<BaseResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogDebug($"Obteniendo el producto con guid: {guid}");
        
        /*
        var cacheKey = CacheKeyPrefix + guid;
        if (_cache.TryGetValue(cacheKey, out BaseModel? cachedBase))
        {
            _logger.LogDebug("Producto obtenido de cache");
            return cachedBase.ToResponseFromModel();
        }*/
        
        var baseEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);

        if (baseEntity != null)
        {
            /*
             cachedBase = BaseMappers.ToModelFromEntity(baseEntity);
             _cache.Set(cacheKey, cachedBase, TimeSpan.FromMinutes(30));
             return cachedBase.ToResponseFromModel();
             */
            _logger.LogInformation($"Producto encontrado con guid: {guid}");
            return BaseMapper.ToResponseFromEntity(baseEntity);
        }
        
        _logger.LogInformation($"Producto no encontrado con guid: {guid}");
        throw new BaseNotExistException($"Producto con guid: {guid} no encontrado");
    }

    public async Task<BaseResponse?> GetByTipoAsync(string tipo)
    {
        _logger.LogInformation($"Buscando producto por tipo: {tipo}");

        /*
            var cacheKey = CacheKeyPrefix + tipo;

            if (_cache.TryGetValue(cacheKey, out BaseModel cachedBase))
            {
                _logger.LogInformation("Producto obtenido desde cache");
                return cachedBase;
            }
        */
        
        var baseEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.TipoProducto.ToLower() == tipo.ToLower());

        if (baseEntity != null)
        {
            /*cachedBase = BaseMappers.ToModelFromEntity(baseEntity);
            _cache.Set(cacheKey, cachedBase, TimeSpan.FromMinutes(30));
            return cachedBase;*/
            
            _logger.LogInformation($"Producto encontrado con el tipo: {tipo}");
            return BaseMapper.ToResponseFromEntity(baseEntity);
        }

        _logger.LogInformation($"Producto no encontrado con el tipo: {tipo}");
        
        throw new BaseNotExistException($"Producto con tipo: {tipo} no encontrado");

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
            _logger.LogWarning($"Ya existe un producto con el nombre: {baseRequest.Nombre}");
            throw new BaseDuplicateException($"Ya existe un producto con el nombre: {baseRequest.Nombre}");
        }
        
        var baseModel = BaseMapper.ToModelFromRequest(baseRequest);
        _context.ProductoBase.Add(BaseMapper.ToEntityFromModel(baseModel));
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Producto creado correctamente con id: {baseModel.Id}");
        return BaseMapper.ToResponseFromModel(baseModel);

    }

    public async Task<BaseResponse?> UpdateAsync(string guid, BaseUpdateDto baseUpdate)
    {
        _logger.LogInformation($"Actualizando producto con guid: {guid}");
        
        var baseEntityExistente = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (baseEntityExistente == null)
        {
            _logger.LogWarning($"Producto con guid: {guid} no encontrado");
            //lanzamos excepcion
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
        baseEntityExistente.TipoProducto = baseUpdate.TipoProducto;
        baseEntityExistente.UpdatedAt = DateTime.UtcNow;

        _context.ProductoBase.Update(baseEntityExistente);
        await _context.SaveChangesAsync();
        
        /*
        var cacheKey = CacheKeyPrefix + id;
        _cache.Remove(cacheKey);
        */

        _logger.LogInformation($"Producto actualizado correctamente con guid: {guid}");
        return BaseMapper.ToResponseFromEntity(baseEntityExistente);
    }

    public async Task<BaseResponse?> DeleteAsync(string guid)
    {
        _logger.LogInformation($"Aplicando borrado logico a producto con guid: {guid}");
        var baseExistenteEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (baseExistenteEntity == null)
        {
            _logger.LogWarning($"Producto con id: {guid} no encontrado");
            throw new BaseNotExistException($"Producto con guid: {guid} no encontrado");
        }

        _logger.LogInformation("Actualizando isDeleted a true");
        baseExistenteEntity.IsDeleted = true;
        baseExistenteEntity.UpdatedAt = DateTime.UtcNow;
        
        _context.ProductoBase.Update(baseExistenteEntity);
        await _context.SaveChangesAsync();
        
        /*
        var cacheKey = CacheKeyPrefix + id;
        _memoryCache.Remove(cacheKey);
        */

        _logger.LogInformation($"Producto borrado logico correctamente con guid: {guid}");
        return BaseMapper.ToResponseFromEntity(baseExistenteEntity);
    }
    
    public async Task<BaseModel?> GetBaseModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando producto con guid: {guid}");

        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Guid == guid);
        if (productoEntity != null)
        {
            _logger.LogInformation($"producto encontrado con guid: {guid}");
            return BaseMapper.ToModelFromEntity(productoEntity);
        }

        _logger.LogInformation($"producto no encontrado con guid: {guid}");
        return null;
    }
        
    public async Task<BaseModel?> GetBaseModelById(long id)
    {
        _logger.LogInformation($"Buscando producto con id: {id}");

        var productoEntity = await _context.ProductoBase.FirstOrDefaultAsync(b => b.Id == id);
        if (productoEntity != null)
        {
            _logger.LogInformation($"producto encontrado con id: {id}");
            return BaseMapper.ToModelFromEntity(productoEntity);
        }

        _logger.LogInformation($"producto no encontrado con id: {id}");
        return null;
    }
    
    //Acemos un GetAllByStorage que es un get all pero sin filtrado ni paginacion que devuelve en modelo
    public async Task<IEnumerable<BaseModel>> GetAllForStorage()
    {
        _logger.LogInformation("Obteniendo todos los productos");
        var productoEntityList = await _context.ProductoBase.ToListAsync();
        //hacemos un bucle para mapear uno a uno
        var baseModelList = new List<BaseModel>();
        foreach (var productoEntity in productoEntityList)
        {
            baseModelList.Add(BaseMapper.ToModelFromEntity(productoEntity));
        }
        return baseModelList;
    }

}