using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vives_Bank_Net.Rest.Database;
using Vives_Bank_Net.Rest.Producto.Base.Database;
using Vives_Bank_Net.Rest.Producto.Base.Dto;
using Vives_Bank_Net.Rest.Producto.Base.Exceptions;
using Vives_Bank_Net.Rest.Producto.Base.Mappers;
using Vives_Bank_Net.Rest.Producto.Base.Models;

namespace Vives_Bank_Net.Rest.Producto.Base.Services;

public class BaseService : IBaseService
{
    private const string CacheKeyPrefix = "Base_";
    private readonly GeneralDbContext _context;
    private readonly ILogger<BaseService> _logger;
    
    public BaseService(GeneralDbContext context, ILogger<BaseService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<BaseModel>> GetAllAsync()
    {
        _logger.LogDebug("Obteniendo todos los productos");
        List<BaseEntity> bases = await _context.ProductoBase.ToListAsync();
        return bases.ToModelList();
    }

    public async Task<BaseResponseDto> GetByGuidAsync(string id)
    {
        _logger.LogDebug($"Obteniendo el producto con id: {id}");
        var cacheKey = CacheKeyPrefix + id;

        /*if (_cache.TryGetValue(cacheKey, out BaseModel? cachedBase))
        {
            _logger.LogDebug("Producto obtenido de cache");
            return cachedBase.ToResponseFromModel();
        }*/
        
        _logger.LogDebug("Buscando producto en la base de datos");
        var model = await _context.ProductoBase.FindAsync(id);

        if (model == null)
        {
            _logger.LogDebug("Producto no encontrado");
            return null;
        }
        
        _logger.LogDebug("Producto obtenido de la base de datos");
        // _cache.Set(cacheKey, model.ToModelFromEntity());
        return model.ToModelFromEntity().ToResponseFromModel();
    }

    public async Task<BaseResponseDto> GetByTipoAsync(string tipo)
    {
        _logger.LogDebug($"Buscando producto por tipo: {tipo}");

        var product = await _context.ProductoBase.FindAsync(tipo);

        if (product == null)
        {
            _logger.LogWarning($"No se encontró ningún producto con el tipo: {tipo}");
            return null;
        }

        _logger.LogDebug($"Producto encontrado con el tipo: {tipo}");

        return product.ToModelFromEntity().ToResponseFromModel();
    }

    public async Task<BaseResponseDto> CreateAsync(BaseRequestDto baseRequest)
    {
        _logger.LogDebug("Creando un nuevo producto base");

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

        var modelEntity = baseRequest.ToEntityFromRequest();
        
        modelEntity.CreatedAt= DateTime.UtcNow;
        modelEntity.UpdatedAt= DateTime.UtcNow;
        
        _context.ProductoBase.Add(modelEntity);
        await _context.SaveChangesAsync();
        
        _logger.LogDebug($"Producto creado correctamente con id: {modelEntity.Id}");

        return modelEntity.ToResponseFromEntity();

    }

    public async Task<BaseResponseDto> UpdateAsync(string id, BaseUpdateDto baseUpdate)
    {
        _logger.LogDebug($"Actualizando producto con id: {id}");
        var model = await _context.ProductoBase.FindAsync(id);

        if (model == null)
        {
            _logger.LogWarning($"Producto con id: {id} no encontrado");
            return null;
        }
    
        _logger.LogDebug("Validando nombre único");
         if (await _context.ProductoBase.AnyAsync(b => b.Nombre.ToLower() == baseUpdate.Nombre.ToLower() && b.Guid!= id))
        {
            _logger.LogWarning($"Ya existe un producto con el nombre: {baseUpdate.Nombre}");
            throw new BaseExistByNameException($"Ya existe un producto con el nombre: {baseUpdate.Nombre}");
        }
        _logger.LogDebug("Actualizando producto");
        model.Nombre = baseUpdate.Nombre;
        model.Descripcion = baseUpdate.Descripcion;
        model.UpdatedAt = DateTime.UtcNow;

        _context.ProductoBase.Update(model);
        await _context.SaveChangesAsync();

        _logger.LogDebug($"Producto actualizado correctamente con id: {id}");
        return model.ToResponseFromEntity();
    }

    public async Task<BaseResponseDto> DeleteAsync(string id)
    {
        _logger.LogDebug($"Aplicando borrado logico a producto con id: {id}");
        var model = await _context.ProductoBase.FindAsync(id);
        if (model == null)
        {
            _logger.LogWarning($"Producto con id: {id} no encontrado");
            return null;
        }

        _logger.LogDebug("Actualizando isDeleted a true");
        model.IsDeleted = true;
        model.UpdatedAt = DateTime.UtcNow;
        _context.ProductoBase.Update(model);
        await _context.SaveChangesAsync();

        _logger.LogDebug($"Producto borrado logico correctamente con id: {id}");
        return model.ToResponseFromEntity();
    }
}