﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Database;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Dto;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Mappers;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Models;
using Vives_Bank_Net.Utils.Validators;
using Vives_Banks_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Services;

public class TarjetaService : ITarjetaService
{

    private const string CacheKeyPrefix = "Tarjeta_";
    private readonly TarjetaDbContext _context;
    private readonly ILogger<TarjetaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly TarjetaGenerator _tarjetaGenerator;
    private readonly CvvGenerator _cvvGenerator;
    private readonly ExpDateGenerator _expDateGenerator;
    private readonly CardLimitValidators _cardLimitValidators;
    

    public TarjetaService(TarjetaDbContext context, ILogger<TarjetaService> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _tarjetaGenerator = new TarjetaGenerator();
        _cvvGenerator = new CvvGenerator();
        _expDateGenerator = new ExpDateGenerator();
        _cardLimitValidators = new CardLimitValidators();
    }
    public async Task<List<TarjetaModel>> GetAllAsync()
    {
       _logger.LogDebug("Obteniendo todas las tarjetas");
       List<TarjetaEntity> tarjetas = await _context.Tarjetas.ToListAsync();
       return tarjetas.ToModelList();
    }

    public async Task<TarjetaResponseDto> GetByGuidAsync(string id)
    {
        _logger.LogDebug($"Obteniendo tarjeta con id: {id}");
        var cacheKey = CacheKeyPrefix + id;

        if (_cache.TryGetValue(cacheKey, out TarjetaModel? cachedTarjeta))
        {
            _logger.LogDebug("Tarjeta obtenida de cache");
            return cachedTarjeta.ToResponseFromModel();
        }
        
        _logger.LogDebug("Buscando tarjeta en la base de datos");
        var tarjeta = await _context.Tarjetas.FindAsync(id);
        if (tarjeta == null)
        {
            _logger.LogDebug("Tarjeta no encontrada");
            return null;
        }
        
        _logger.LogDebug("Tarjeta obtenida de la base de datos");
        _cache.Set(cacheKey, tarjeta.ToModelFromEntity());
        return tarjeta.ToModelFromEntity().ToResponseFromModel();
    }

    public async Task<TarjetaResponseDto> CreateAsync(TarjetaRequestDto dto)
    {
        _logger.LogDebug("Creando una nueva tarjeta");

        var tarjeta = dto.ToModelFromRequest();

        if (!_cardLimitValidators.ValidarLimite(dto))
        {
            _logger.LogWarning("Los límites de la tarjeta no son válidos");
            return null;
        }
        
        tarjeta.Numero = _tarjetaGenerator.GenerarTarjeta();
        tarjeta.Cvv = _cvvGenerator.GenerarCvv();
        tarjeta.FechaVencimiento = _expDateGenerator.GenerarExpDate();
        
        var tarjetaEntity = tarjeta.ToEntityFromModel();
        
        _context.Tarjetas.Add(tarjetaEntity);
        await _context.SaveChangesAsync();
        
        _logger.LogDebug("Tarjeta creada correctamente con id: {tarjetaEntity.Id}");

        return tarjetaEntity.ToResponseFromEntity();
    }

    public async Task<TarjetaResponseDto> UpdateAsync(string id, TarjetaRequestDto dto)
    {
        _logger.LogDebug($"Actualizando tarjeta con id: {id}");
        var tarjeta = await _context.Tarjetas.FindAsync(id);
        
        if (tarjeta == null)
        {
            _logger.LogWarning($"Tarjeta con id: {id} no encontrada");
            return null;
        }
        
        if (!_cardLimitValidators.ValidarLimite(dto))
        {
            _logger.LogWarning("Los límites de la tarjeta no son válidos");
            return null;
        }

        _logger.LogDebug("Actualizando tarjeta");
        tarjeta.Pin = dto.Pin;
        tarjeta.LimiteDiario = dto.LimiteDiario;
        tarjeta.LimiteSemanal = dto.LimiteSemanal;
        tarjeta.LimiteMensual = dto.LimiteMensual;

        _context.Tarjetas.Update(tarjeta);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Tarjeta actualizada correctamente con id: {id}");
        return tarjeta.ToResponseFromEntity();
    }

    public async Task<TarjetaResponseDto> DeleteAsync(string id)
    {
        _logger.LogDebug($"Eliminando tarjeta con id: {id}");
        var tarjeta = await _context.Tarjetas.FindAsync(id);

        if (tarjeta == null)
        {
            _logger.LogWarning($"Tarjeta con id: {id} no encontrada");
            return null;
        }

        _logger.LogDebug("Eliminando tarjeta");
        tarjeta.IsDeleted = true;

        _context.Tarjetas.Update(tarjeta);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Tarjeta eliminada correctamente con id: {id}");
        return tarjeta.ToResponseFromEntity();
    }
}