using System.Numerics;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
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

    
    public async Task<PageResponse<CuentaResponse>> GetAllAsync(
    
    BigInteger? saldoMax,
    BigInteger? saldoMin,
    String? tipoCuenta,
    PageRequest pageRequest)
    {/*
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
        Content = content.Select(c =>
        {
            return CuentaMapper.ToResponseFromEntity(c);
        }).ToList(),
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

    return pageResponse;*/
        return null;
    }

    public async Task<IEnumerable<CuentaResponse>> GetByClientGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando todas las Cuentas del cliente con guid: {guid}");
        
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Guid == guid);
        if (!clienteExiste)
        {
            throw new ClienteNotFound($"No se encontró el cliente con guid: {guid}");
        }
        
        var cuentaEntityList = await _context.Cuentas
            .Where(c => c.Cliente.Guid == guid)
            .ToListAsync();

        var cuentaResponses = new List<CuentaResponse>();
        
        foreach (var cuentaEntity in cuentaEntityList)
        {
            var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
            var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
            var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);

            var cuentaResponse = CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
            cuentaResponses.Add(cuentaResponse);
        }

        return cuentaResponses;
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
            var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
            var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
            var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);
            _logger.LogInformation($"Cuenta encontrada con guid: {guid}");
            return CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
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
            var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
            var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
            var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);
            _logger.LogInformation($"Cuenta encontrada con iban: {iban}");
            return CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }
    
    public async Task<CuentaResponse?> GetMeByIbanAsync(string guid,string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban}");
        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuentaEntity != null)
        {
            var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
            var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
            var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);
            _logger.LogInformation($"Cuenta encontrada con iban: {iban}");
            return CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
        }

        if (cuentaEntity.Cliente.Guid == guid)
        {
            var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
            var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
            var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);
            _logger.LogInformation($"Cuenta con IBAN: {iban}  le pertenece");
            return CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }

    public async Task<CuentaResponse> CreateAsync(string guid,CuentaRequest cuentaRequest)
    {
        _logger.LogInformation($"Creando cuenta nueva");
        var tipoCuenta = _baseService.GetByTipoAsync(cuentaRequest.TipoCuenta);
        
        if (tipoCuenta == null)
        {
            _logger.LogError($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            throw new BaseNotExistException($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            
        }
        //TODO
        //buscar el cliente y pasarle el id
        //Pasarle el clienteid
        var cuenta =crearCuenta(tipoCuenta.Id,0);
        
        var cuentaEntity = CuentaMapper.ToEntityFromModel(cuenta);

        await _context.Cuentas.AddAsync(cuentaEntity);

        await _context.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntity.TarjetaModel.Id);
        var cliente = await _clienteService.GetClienteModelById(cuentaEntity.Cliente.Id);
        var producto = await _baseService.GetBaseModelById(cuentaEntity.ProductoId);
        var cuentaResponse = CuentaMapper.ToResponseFromEntity(cuentaEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
        
        return cuentaResponse;
    }

    private Banco_VivesBank.Producto.Cuenta.Models.Cuenta crearCuenta(long idProducto, long idCliente)
    {
     
        return new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            ProductoId = idProducto,
            ClienteId = idCliente
        };

    }

    public async Task<CuentaResponse?> UpdateAsync(string guidClient,string guid, CuentaUpdateRequest cuentaRequest)
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
            else
            {
             cuentaEntityExistente.Saldo = saldo;   
            }
            
        }
        else
        {
            _logger.LogError($"El saldo proporcionado no es válido.");
            throw new SaldoInvalidoException("El saldo proporcionado no es válido.");
        }

        await _context.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaEntityExistente.TarjetaModel.Id);
        var cliente = await _clienteService.GetClienteModelById(cuentaEntityExistente.Cliente.Id);
        var producto = await _baseService.GetBaseModelById(cuentaEntityExistente.ProductoId);
        var cuentaResponse = CuentaMapper.ToResponseFromEntity(cuentaEntityExistente, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));

        return cuentaResponse;
    }


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
        var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaExistenteEntity.TarjetaModel.Id);
        var cliente = await _clienteService.GetClienteModelById(cuentaExistenteEntity.Cliente.Id);
        var producto = await _baseService.GetBaseModelById(cuentaExistenteEntity.ProductoId);
        return CuentaMapper.ToResponseFromEntity(cuentaExistenteEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
    }
    
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
        var tarjeta = await _tarjetaService.GetTarjetaModelById(cuentaExistenteEntity.TarjetaModel.Id);
        var cliente = await _clienteService.GetClienteModelById(cuentaExistenteEntity.Cliente.Id);
        var producto = await _baseService.GetBaseModelById(cuentaExistenteEntity.ProductoId);
        return CuentaMapper.ToResponseFromEntity(cuentaExistenteEntity, TarjetaMappers.ToResponseFromModel(tarjeta), ClienteMapper.ToResponseFromModel(cliente), BaseMapper.ToResponseFromModel(producto));
    }

    public async Task<Models.Cuenta?> GetCuentaModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando cuenta con guid: {guid}");

        var cuentaEntity = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);
        if (cuentaEntity != null)
        {
            _logger.LogInformation($"cuenta encontrada con guid: {guid}");
            return CuentaMapper.ToModelFromEntity(cuentaEntity);
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
            return CuentaMapper.ToModelFromEntity(cuentaEntity);
        }

        _logger.LogInformation($"Cuenta no encontrada con id: {id}");
        return null;
    }
}