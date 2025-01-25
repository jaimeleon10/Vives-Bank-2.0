using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public class TarjetaService : ITarjetaService
{

    private readonly GeneralDbContext _context;
    private readonly ILogger<TarjetaService> _logger;
    private readonly ILogger<CardLimitValidators> _log;
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _redis;
    private readonly CardLimitValidators _cardLimitValidators;
    private const string CacheKeyPrefix = "Tarjeta:"; 
    

    public TarjetaService(GeneralDbContext context, ILogger<TarjetaService> logger, ILogger<CardLimitValidators> log,  IConnectionMultiplexer redis,  IMemoryCache memoryCache )
    {
        _context = context;
        _logger = logger;
        _log = log;
        _redis = redis;
        _memoryCache = memoryCache; 
        _cardLimitValidators = new CardLimitValidators(_log);
        _database = _redis.GetDatabase();
    }

    public async Task<PageResponse<TarjetaResponse>> GetAllPagedAsync(PageRequest page)
    {
        _logger.LogInformation("Obteniendo todas las tarjetas paginadas");
        int pageNumber = page.PageNumber >= 0 ? page.PageNumber : 0;
        int pageSize = page.PageSize > 0 ? page.PageSize : 10;

        var query = _context.Tarjetas.AsQueryable();

        query = page.SortBy.ToLower() switch
        {
            "id" => page.Direction.ToUpper() == "ASC"
                ? query.OrderBy(t => t.Id)
                : query.OrderByDescending(t => t.Id),
            _ => throw new InvalidOperationException($"La propiedad {page.SortBy} no es válida para ordenamiento.")
        };

        query = query.OrderBy(t => t.Id);

        var totalElements = await query.CountAsync();
        
        var content = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        
        var totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
        
        var contentResponse = content.Select(TarjetaMapper.ToResponseFromEntity).ToList();
        
        return new PageResponse<TarjetaResponse>
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

    public async Task<TarjetaResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogDebug($"Obteniendo tarjeta con id: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Tarjeta? cachedTarjeta))
        {
            _logger.LogInformation("Tarjeta obtenida desde la memoria caché");
            return TarjetaMapper.ToResponseFromModel(cachedTarjeta!);
        }
        //Redis
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!redisCache.IsNullOrEmpty)
        {
            _logger.LogInformation("Tarjeta obtenido desde Redis");
            var tarjetaRedis = JsonSerializer.Deserialize<Models.Tarjeta>(redisCache!);
            if (tarjetaRedis == null)
            {
                _logger.LogWarning("Error al deserializar tarjeta desde Redis");
                throw new Exception("Error al deserializar tarjeta desde Redis");
            }
            _memoryCache.Set(cacheKey, tarjetaRedis, TimeSpan.FromMinutes(30));
            return tarjetaRedis.ToResponseFromModel(); 
        }
        
        // Consultar la base de datos
        var tarjetaEntity = await _context.Tarjetas.FirstOrDefaultAsync(u => u.Guid == guid);
        if (tarjetaEntity != null)
        {
            _logger.LogInformation($"Tarjeta no encontrada con guid: {guid}");

            // Mapear entidad a modelo y respuesta
            var tarjetaResponse = TarjetaMapper.ToResponseFromEntity(tarjetaEntity);
            var tarjetaModel = TarjetaMapper.ToModelFromEntity(tarjetaEntity);

            // Guardar en la memoria caché
            _memoryCache.Set(cacheKey, tarjetaModel, TimeSpan.FromMinutes(30));

            // Guardar en Redis como JSON
            var redisValue = JsonSerializer.Serialize(tarjetaModel);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return tarjetaResponse;
        }

        _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
        return null;
    }

    public async Task<TarjetaResponse?> GetByNumeroTarjetaAsync(string numeroTarjeta)
    {
        _logger.LogDebug($"Obteniendo tarjeta con número de tarjeta: {numeroTarjeta}");
        
        var tarjetaEntity = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Numero == numeroTarjeta);
        if (tarjetaEntity != null)
        {
            _logger.LogInformation($"Tarjeta encontrada con numero: {numeroTarjeta}");
            return tarjetaEntity.ToResponseFromEntity();
        }
        
        _logger.LogInformation($"Tarjeta no encontrada con numero: {numeroTarjeta}");
        return null;
    }

    public async Task<TarjetaResponse> CreateAsync(TarjetaRequest tarjetaRequest)
    {
        _logger.LogDebug("Creando una nueva tarjeta");
        
        _logger.LogInformation($"Validando que la cuenta con guid {tarjetaRequest.CuentaGuid} existe");
        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == tarjetaRequest.CuentaGuid);
        if (cuentaEntity == null)
        {
            _logger.LogWarning($"Cuenta con guid: {tarjetaRequest.CuentaGuid} no encontrada");
            throw new CuentaNotFoundException("Cuenta no encontrada");
        }
        
        _logger.LogInformation($"Validando que la cuenta con guid {tarjetaRequest.CuentaGuid} no tiene tarjeta asignada");
        if (cuentaEntity.TarjetaId != null)
        {
            _logger.LogWarning($"La cuenta con guid: {cuentaEntity.Guid} ya tiene una tarjeta asignada");
            throw new CuentaException($"La cuenta con guid: {cuentaEntity.Guid} ya tiene una tarjeta asignada");
        }
        
        _cardLimitValidators.ValidarLimite(tarjetaRequest.LimiteDiario, tarjetaRequest.LimiteSemanal, tarjetaRequest.LimiteMensual);

        var tarjetaEntity = new TarjetaEntity
        {
            Pin = tarjetaRequest.Pin,
            LimiteDiario = tarjetaRequest.LimiteDiario,
            LimiteSemanal = tarjetaRequest.LimiteSemanal,
            LimiteMensual = tarjetaRequest.LimiteMensual,
        };
        
        _context.Tarjetas.Add(tarjetaEntity);
        await _context.SaveChangesAsync();
        
        cuentaEntity.TarjetaId = tarjetaEntity.Id;
        _context.Cuentas.Update(cuentaEntity);
        await _context.SaveChangesAsync();
        
        var tarjetaModel = tarjetaEntity.ToModelFromEntity();
        var cacheKey = CacheKeyPrefix + tarjetaModel.Guid;
        _memoryCache.Set(cacheKey, tarjetaModel);
        
        // Guardar la tarjeta 
        var serializedUser = JsonSerializer.Serialize(tarjetaModel);
        await _database.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));
        
        _logger.LogDebug($"Tarjeta creada correctamente con id: {tarjetaEntity.Id}");

        return tarjetaEntity.ToResponseFromEntity();
    }

    public async Task<TarjetaResponse?> UpdateAsync(string guid, TarjetaRequestUpdate dto)
    {
        _logger.LogDebug($"Actualizando tarjeta con id: {guid}");
        var tarjeta = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Guid == guid);
        
        if (tarjeta == null)
        {
            _logger.LogWarning($"Tarjeta con id: {guid} no encontrada");
            return null;
        }
        
        _cardLimitValidators.ValidarLimite(dto.LimiteDiario, dto.LimiteSemanal, dto.LimiteMensual);

        _logger.LogDebug("Actualizando tarjeta");
        tarjeta.Pin = dto.Pin;
        tarjeta.LimiteDiario = dto.LimiteDiario;
        tarjeta.LimiteSemanal = dto.LimiteSemanal;
        tarjeta.LimiteMensual = dto.LimiteMensual;
        tarjeta.UpdatedAt = DateTime.UtcNow;
        //tarjeta.IsDeleted = dto.IsDeleted;

        _context.Tarjetas.Update(tarjeta);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + tarjeta.Guid;
        
        _memoryCache.Remove(cacheKey);  
        await _database.KeyDeleteAsync(cacheKey);  
        
        var tarjetaModel = tarjeta.ToModelFromEntity();
        
        var serializedUser = JsonSerializer.Serialize(tarjetaModel);
        _memoryCache.Set(cacheKey, tarjetaModel, TimeSpan.FromMinutes(30));  
        await _database.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30)); 


        _logger.LogDebug($"Tarjeta actualizada correctamente con id: {guid}");
        return tarjeta.ToResponseFromEntity();
    }

    public async Task<TarjetaResponse?> DeleteAsync(string guid)
    {
        _logger.LogDebug($"Eliminando tarjeta con guid: {guid}");
        var tarjeta = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Guid == guid);

        if (tarjeta == null)
        {
            _logger.LogWarning($"Tarjeta con guid: {guid} no encontrada");
            return null;
        }

        _logger.LogDebug("Actualizando isDeleted a true");
        tarjeta.IsDeleted = true;
        tarjeta.UpdatedAt = DateTime.UtcNow;

        _context.Tarjetas.Update(tarjeta);
        await _context.SaveChangesAsync();

        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Tarjeta!.Guid == guid);
        cuentaEntity!.TarjetaId = null;
        cuentaEntity.Tarjeta = null;
        _context.Cuentas.Update(cuentaEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + tarjeta.Guid;
        
        _memoryCache.Remove(cacheKey);
        await _database.KeyDeleteAsync(cacheKey);

        _logger.LogDebug($"Tarjeta eliminada correctamente con guid: {guid}");
        return tarjeta.ToResponseFromEntity();
    }
    
    public async Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Tarjeta con guid: {guid}");

        var tarjetaEntity = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Guid == guid);
        if (tarjetaEntity != null)
        {
            _logger.LogInformation($"Tarjeta encontrada con guid: {guid}");
            return TarjetaMapper.ToModelFromEntity(tarjetaEntity);
        }

        _logger.LogInformation($"Tarjeta no encontrada con guid: {guid}");
        return null;
    }
        
    public async Task<Models.Tarjeta?> GetTarjetaModelById(long id)
    {
        _logger.LogInformation($"Buscando Tarjeta con id: {id}");

        var tarjetaEntity = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == id);
        if (tarjetaEntity != null)
        {
            _logger.LogInformation($"Tarjeta encontrada con id: {id}");
            return TarjetaMapper.ToModelFromEntity(tarjetaEntity);
        }

        _logger.LogInformation($"Tarjeta no encontrada con id: {id}");
        return null;
    }
    
    public async Task<List<Models.Tarjeta>> GetAllForStorage()
    {
        _logger.LogDebug("Obteniendo todas las tarjetas");
        List<TarjetaEntity> tarjetas = await _context.Tarjetas.ToListAsync();
        return tarjetas.Select(t => TarjetaMapper.ToModelFromEntity(t)).ToList();
    }
}