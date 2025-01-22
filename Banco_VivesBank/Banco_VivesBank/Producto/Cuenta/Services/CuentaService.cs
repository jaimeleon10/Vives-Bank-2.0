using System.Numerics;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public class CuentaService : ICuentaService
{
    
    private readonly GeneralDbContext _context;
    private readonly IBaseService _baseService;
    private readonly ILogger<CuentaService> _logger;
    private readonly ITarjetaService _tarjetaService;
    private readonly IClienteService _clienteService;

    public CuentaService(GeneralDbContext context, ILogger<CuentaService> logger, IBaseService baseService, IClienteService clienteService, ITarjetaService tarjetaService)
    {
        _context = context;
        _logger = logger;
        _baseService = baseService;
        _clienteService = clienteService;
        _tarjetaService = tarjetaService;
    }

    
    public async Task<PageResponse<CuentaResponse>> GetAllAsync(BigInteger? saldoMax, BigInteger? saldoMin, string? tipoCuenta, PageRequest pageRequest)
    {
        _logger.LogInformation("Buscando todos las Cuentas en la base de datos");
        int pageNumber = pageRequest.PageNumber >= 0 ? pageRequest.PageNumber : 0;
        int pageSize = pageRequest.PageSize > 0 ? pageRequest.PageSize : 10;

        var query = _context.Cuentas.AsQueryable();

        if (saldoMax.HasValue)
        {
            _logger.LogInformation($"Filtrando por Saldo Maximo: {saldoMax}");
            query = query.Where(c => c.Saldo <= saldoMax.Value);
        }

        if (saldoMin.HasValue)
        {
            _logger.LogInformation($"Filtrando por Saldo Minimo: {saldoMax}");
            query = query.Where(c => c.Saldo >= saldoMin.Value);
        }

        if (!string.IsNullOrEmpty(tipoCuenta))
        {
            _logger.LogInformation($"Filtrando por Tipo de cuenta: {tipoCuenta}");
            query = query.Where(c => c.Producto.Nombre.ToString().Contains(tipoCuenta));
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

        var totalPages = (int)Math.Ceiling((double) totalElements / pageSize);
        
        var pageResponse = new PageResponse<CuentaResponse>
        {
            Content = content.Select(entity => entity.ToResponseFromEntity() ).ToList(),
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

    public async Task<IEnumerable<CuentaResponse>> GetByClientGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando todas las Cuentas del cliente con guid: {guid}");
        
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Guid == guid);
        if (!clienteExiste)
        {
            _logger.LogInformation($"Cliente con guid: {guid} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guid}");
        }
        var query = _context.Cuentas.AsQueryable().Where(c => c.Cliente.Guid == guid); 
        var content = await query.ToListAsync();
        
        var cuentasResponses = content.Select(c => c.ToResponseFromEntity()).ToList();
        
        return cuentasResponses;
    }

    public async Task<CuentaResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando Cuenta con guid: {guid} en la base de datos");
        
        /*
        var cacheKey = CacheKeyPrefix + guid;
        if (_cache.TryGetValue(cacheKey, out Cuenta? cachedCuenta))
        {
            _logger.LogDebug("Cuenta obtenida de cache");
            return cachedCuenta.ToResponseFromModel();
        }*/
        
        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);

        if (cuentaEntity != null)
        {
            /*
             cachedCuenta = CuentaMapper.ToModelFromEntity(cuentaEntity);
             _cache.Set(cacheKey, cachedCuenta, TimeSpan.FromMinutes(30));
             return cachedCuenta.ToResponseFromModel();
             */
            _logger.LogInformation($"Cuenta encontrada con guid: {guid}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con guid: {guid}");
        return null;
    }

    public async Task<CuentaResponse?> GetByIbanAsync(string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban} en la base de datos");
        
        /*
        var cacheKey = CacheKeyPrefix + iban;
        if (_cache.TryGetValue(cacheKey, out Cuenta? cachedCuenta))
        {
            _logger.LogDebug("Cuenta obtenida de cache");
            return cachedCuenta;
        }
        */
        
        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuentaEntity != null)
        {
            /*
             cachedCuenta = CuentaMapper.ToModelFromEntity(cuentaEntity);
             _cache.Set(cacheKey, cachedCuenta, TimeSpan.FromMinutes(30));
             return cachedCuenta.ToResponseFromModel();
             */
            _logger.LogInformation($"Cuenta encontrada con iban: {iban}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }
    /*
    public async Task<CuentaResponse?> GetMeByIbanAsync(string guid, string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban}");
        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuentaEntity != null)
        {
            if (cuentaEntity.Cliente.Guid != guid)
            {
                _logger.LogInformation($"La cuenta con el iban {iban} no pertenece al cliente que la solicita");
                throw new ClienteException($"La cuenta con el iban {iban} no pertenece al cliente que la solicita");
            }
            _logger.LogInformation($"Cuenta encontrada con guid: {guid}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }
    */
/*
    public async Task<CuentaResponse> CreateAsync(string guid, CuentaRequest cuentaRequest)
    {
        _logger.LogInformation($"Creando cuenta nueva");
        var tipoCuenta = await _baseService.GetByTipoAsync(cuentaRequest.TipoCuenta);
        
        if (tipoCuenta == null)
        {
            _logger.LogError($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            throw new BaseNotExistException($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            
        }

        var tipoCuentaModel = await _baseService.GetBaseModelByGuid(tipoCuenta.Guid);
        var clienteModel = await _clienteService.GetClienteModelByGuid(cuentaRequest.ClienteGuid);
        
        var cuenta = new Models.Cuenta
        {
            Producto = tipoCuentaModel,
            Cliente = clienteModel
        };

        var cuentaEntity = CuentaMapper.ToEntityFromModel(cuenta);

        await _context.Cuentas.AddAsync(cuentaEntity);
        await _context.SaveChangesAsync();

        var cuentaResponse = CuentaMapper.ToResponseFromModel(cuenta, cuenta.Tarjeta.Guid, cuenta.Cliente.Guid, cuenta.Producto.Guid);
        
        return cuentaResponse;
        return null;
    }
*/
    
    public async Task<CuentaResponse> CreateAsync( CuentaRequest cuentaRequest)
    {
        _logger.LogInformation($"Creando cuenta nueva");
        var tipoCuenta = await _baseService.GetByTipoAsync(cuentaRequest.TipoCuenta);
        
        if (tipoCuenta == null)
        {
            _logger.LogError($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            throw new BaseNotExistException($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            
        }

        var tipoCuentaModel = await _baseService.GetBaseModelByGuid(tipoCuenta.Guid);
        var clienteModel = await _clienteService.GetClienteModelByGuid(cuentaRequest.ClienteGuid);
        
        var cuenta = new Models.Cuenta
        {
            Producto = tipoCuentaModel,
            Cliente = clienteModel
        };

        var cuentaEntity = cuenta.ToEntityFromModel();

        await _context.Cuentas.AddAsync(cuentaEntity);
        await _context.SaveChangesAsync();

        var cuentaResponse = cuentaEntity.ToResponseFromEntity();
        
        return cuentaResponse;
    }
    
    /*
    public async Task<CuentaResponse?> UpdateAsync(string guidClient, string guid, CuentaUpdateRequest cuentaRequest)
    {
        _logger.LogInformation($"Actualizando cuenta con guid: {guid}");
        
        var cuentaEntityExistente = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  
        if (cuentaEntityExistente == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            return null;
        }
        
        _logger.LogDebug("Validando guid del cliente");
        if (cuentaEntityExistente.Cliente.Guid != guidClient)
        {
            _logger.LogError($"Cuenta con IBAN: {cuentaEntityExistente.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuentaEntityExistente.Iban}  no le pertenece");
        }

        _logger.LogInformation("Actualizando cuenta");
        if (BigInteger.TryParse(cuentaRequest.Saldo, out var saldo))
        {
            if (saldo <= 0)
            {
                _logger.LogError($"Saldo insuficiente para actualizar.");
                throw new SaldoInsuficienteException("El saldo debe ser mayor a 0.");
            }
             
            cuentaEntityExistente.Saldo = saldo;   
            
        }
        else
        {
            _logger.LogError($"El saldo proporcionado no es válido.");
            throw new SaldoInvalidoException("El saldo proporcionado no es válido.");
        }

        _context.Cuentas.Update(cuentaEntityExistente);
        await _context.SaveChangesAsync();
        
        var cuentaResponse = cuentaEntityExistente.ToResponseFromEntity();

        return cuentaResponse;
    }
    */
    public async Task<CuentaResponse?> UpdateAsync(string guid, CuentaUpdateRequest cuentaRequest)
    {
        _logger.LogInformation($"Actualizando cuenta con guid: {guid}");
        
        var cuentaEntityExistente = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  
        if (cuentaEntityExistente == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            return null;
        }
        
        _logger.LogDebug("Validando guid del cliente");
        if (cuentaEntityExistente.Cliente.Guid != cuentaRequest.ClienteGuid)
        {
            _logger.LogError($"Cuenta con IBAN: {cuentaEntityExistente.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuentaEntityExistente.Iban}  no le pertenece");
        }

        _logger.LogInformation("Actualizando cuenta");
        if (BigInteger.TryParse(cuentaRequest.Saldo, out var saldo))
        {
            if (saldo <= 0)
            {
                _logger.LogError($"Saldo insuficiente para actualizar.");
                throw new SaldoInsuficienteException("El saldo debe ser mayor a 0.");
            }
             
            cuentaEntityExistente.Saldo = saldo;   
            
        }
        else
        {
            _logger.LogError($"El saldo proporcionado no es válido.");
            throw new SaldoInvalidoException("El saldo proporcionado no es válido.");
        }

        _context.Cuentas.Update(cuentaEntityExistente);
        await _context.SaveChangesAsync();
        
        var cuentaResponse = cuentaEntityExistente.ToResponseFromEntity();

        return cuentaResponse;
    }

    /*
    public async Task<CuentaResponse?> DeleteAsync(string guidClient,string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        
        var cuentaExistenteEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  
        if (cuentaExistenteEntity == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            return null;
        }

        if (cuentaExistenteEntity.Cliente.Guid != guidClient)
        {
            _logger.LogError($"Cuenta con IBAN: {cuentaExistenteEntity.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuentaExistenteEntity.Iban}  no le pertenece");
        }

        _logger.LogInformation("Actualizando isDeleted a true");
        cuentaExistenteEntity.IsDeleted = true;
        cuentaExistenteEntity.UpdatedAt = DateTime.UtcNow;

        _context.Cuentas.Update(cuentaExistenteEntity);
        await _context.SaveChangesAsync();

        
        //var cacheKey = CacheKeyPrefix + id;
       // _memoryCache.Remove(cacheKey);
       
        
        _logger.LogInformation($"Cuenta borrada correctamente con guid: {guid}");
        return cuentaExistenteEntity.ToResponseFromEntity();
    }
*/
    
    public async Task<CuentaResponse?> DeleteAdminAsync(string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        
        var cuentaExistenteEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  
        if (cuentaExistenteEntity == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            return null;
        }

        _logger.LogInformation("Actualizando isDeleted a true");
        cuentaExistenteEntity.IsDeleted = true;
        cuentaExistenteEntity.UpdatedAt = DateTime.UtcNow;

        _context.Cuentas.Update(cuentaExistenteEntity);
        await _context.SaveChangesAsync();

        /*
        var cacheKey = CacheKeyPrefix + id;
        _memoryCache.Remove(cacheKey);
        */
        
        _logger.LogInformation($"Cuenta borrada correctamente con guid: {guid}");
        return cuentaExistenteEntity.ToResponseFromEntity();
    }

    public async Task<Models.Cuenta?> GetCuentaModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando cuenta con guid: {guid}");

        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);
        if (cuentaEntity != null)
        {
            _logger.LogInformation($"cuenta encontrada con guid: {guid}");
            return cuentaEntity.ToModelFromEntity();
        }

        _logger.LogInformation($"cuenta no encontrada con guid: {guid}");
        return null;
    }
        
    public async Task<Models.Cuenta?> GetCuentaModelById(long id)
    {
        _logger.LogInformation($"Buscando usuario con id: {id}");

        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == id);
        if (cuentaEntity != null)
        {
            _logger.LogInformation($"Cuenta encontrada con id: {id}");
            return cuentaEntity.ToModelFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con id: {id}");
        return null;
    }
}