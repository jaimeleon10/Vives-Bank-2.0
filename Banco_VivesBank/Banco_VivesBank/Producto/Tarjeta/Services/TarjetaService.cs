using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
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
    private readonly TarjetaGenerator _tarjetaGenerator;
    private readonly CvvGenerator _cvvGenerator;
    private readonly ExpDateGenerator _expDateGenerator;
    private readonly CardLimitValidators _cardLimitValidators;
    private const string CacheKeyPrefix = "Tarjeta:"; 
    

    public TarjetaService(GeneralDbContext context, ILogger<TarjetaService> logger, ILogger<CardLimitValidators> log,  IConnectionMultiplexer redis,  IMemoryCache memoryCache )
    {
        _context = context;
        _logger = logger;
        _log = log;
        _redis = redis;
        _memoryCache = memoryCache; 
        _tarjetaGenerator = new TarjetaGenerator();
        _cvvGenerator = new CvvGenerator();
        _expDateGenerator = new ExpDateGenerator();
        _cardLimitValidators = new CardLimitValidators(_log);
        _database = _redis.GetDatabase();
    }

    public async Task<List<TarjetaResponse>> GetAllAsync()
    {
       _logger.LogDebug("Obteniendo todas las tarjetas");
       List<TarjetaEntity> tarjetas = await _context.Tarjetas.ToListAsync();
       return tarjetas.ToResponseList();
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
            _logger.LogInformation("Tarjeta obtenido desde la memoria caché");
            return TarjetaMapper.ToResponseFromModel(cachedTarjeta);
        }
        //Redis
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Tarjeta obtenido desde Redis");
            var tarjetaResponse = JsonSerializer.Deserialize<TarjetaResponse>(redisCache);
            if (tarjetaResponse != null)
            {
                _memoryCache.Set(cacheKey, tarjetaResponse, TimeSpan.FromMinutes(30));
            }

            return tarjetaResponse; 
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
            var redisValue = JsonSerializer.Serialize(tarjetaResponse);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

            return tarjetaResponse;
        }

        _logger.LogInformation($"Usuario no encontrado con guid: {guid}");
        return null;
    }

    public async Task<TarjetaResponse?> GetByNumeroTarjetaAsync(string numeroTarjeta)
    {
        _logger.LogDebug($"Obteniendo tarjeta con número de tarjeta: {numeroTarjeta}");
        
        var cacheKey = CacheKeyPrefix + numeroTarjeta;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Tarjeta? cachedTarjeta))
        {
            _logger.LogDebug("Tarjeta obtenida de cache en memoria");
            return cachedTarjeta.ToResponseFromModel();
        }
        
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(redisCache))
        {
            _logger.LogInformation("Tarjeta obtenida desde Redis");
            var tarjetaResponseFromRedis = JsonSerializer.Deserialize<TarjetaResponse>(redisCache);
            if (tarjetaResponseFromRedis != null)
            {
                _memoryCache.Set(cacheKey, tarjetaResponseFromRedis, TimeSpan.FromMinutes(30));
            }

            return tarjetaResponseFromRedis;
        }
        
        _logger.LogDebug("Buscando tarjeta en la base de datos");
        var tarjetaEntity = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Numero == numeroTarjeta);
        if (tarjetaEntity != null)
        {
            var tarjetaResponse = tarjetaEntity.ToResponseFromEntity();
            var tarjeta = tarjetaEntity.ToModelFromEntity();
            
            _memoryCache.Set(cacheKey, tarjeta, TimeSpan.FromMinutes(30));

            var redisValue = JsonSerializer.Serialize(tarjetaResponse);
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation($"Tarjeta encontrada con numero: {numeroTarjeta}");
            return tarjetaResponse;
        }
        
        _logger.LogInformation($"Tarjeta no encontrada con numero: {numeroTarjeta}");
        return null;
    }

    public async Task<TarjetaResponse> CreateAsync(TarjetaRequest tarjetaRequest)
    {
        _logger.LogDebug("Creando una nueva tarjeta");

        var tarjeta = tarjetaRequest.ToModelFromRequest();
        
        tarjeta.Numero = _tarjetaGenerator.GenerarTarjeta();
        tarjeta.Cvv = _cvvGenerator.GenerarCvv();
        tarjeta.FechaVencimiento = _expDateGenerator.GenerarExpDate();
        
        var tarjetaEntity = tarjeta.ToEntityFromModel();
        
        _context.Tarjetas.Add(tarjetaEntity);
        await _context.SaveChangesAsync();
        
        var tarjetaModel = tarjetaEntity.ToModelFromEntity();
        var cacheKey = CacheKeyPrefix + tarjetaModel.Guid;
        _memoryCache.Set(cacheKey, tarjetaModel);
        
        // Guardar la tarjeta 
        var serializedUser = JsonSerializer.Serialize(tarjetaModel.ToResponseFromModel());
        await _database.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));
        
        _logger.LogDebug($"Tarjeta creada correctamente con id: {tarjetaEntity.Id}");

        return tarjetaEntity.ToResponseFromEntity();
    }

    public async Task<TarjetaResponse?> UpdateAsync(string guid, TarjetaRequest dto)
    {
        _logger.LogDebug($"Actualizando tarjeta con id: {guid}");
        var tarjeta = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Guid == guid);
        
        if (tarjeta == null)
        {
            _logger.LogWarning($"Tarjeta con id: {guid} no encontrada");
            return null;
        }

        _cardLimitValidators.ValidarLimite(dto);

        _logger.LogDebug("Actualizando tarjeta");
        tarjeta.Pin = dto.Pin;
        tarjeta.LimiteDiario = dto.LimiteDiario;
        tarjeta.LimiteSemanal = dto.LimiteSemanal;
        tarjeta.LimiteMensual = dto.LimiteMensual;
        tarjeta.UpdatedAt = DateTime.UtcNow;

        _context.Tarjetas.Update(tarjeta);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + tarjeta.ToModelFromEntity().Guid;
        
        _memoryCache.Remove(cacheKey);  
        await _database.KeyDeleteAsync(cacheKey);  
        
        var tarjetaModel = tarjeta.ToModelFromEntity();
        
        var serializedUser = JsonSerializer.Serialize(tarjeta);
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
        
        var cacheKey = CacheKeyPrefix + tarjeta.ToModelFromEntity().Guid;
        
        _memoryCache.Remove(cacheKey);
        await _database.KeyDeleteAsync(cacheKey);

        _logger.LogDebug($"Tarjeta eliminada correctamente con guid: {guid}");
        return tarjeta.ToResponseFromEntity();
    }
    
    public async Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Tarjeta con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Tarjeta? cacheTarjeta))
        {
            _logger.LogInformation("Usuario obtenido desde la memoria caché");
            return cacheTarjeta;
        }

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
        
        var cacheKey = CacheKeyPrefix + id;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Tarjeta? cacheTarjeta))
        {
            _logger.LogInformation("Usuario obtenido desde la memoria caché");
            return cacheTarjeta;
        }

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