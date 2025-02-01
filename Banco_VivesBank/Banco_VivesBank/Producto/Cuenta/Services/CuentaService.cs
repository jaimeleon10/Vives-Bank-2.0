using System.Numerics;
using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.Producto.ProductoBase.Mappers;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Generators;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Websockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public class CuentaService : ICuentaService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly IProductoService _productoService;
    private readonly ILogger<CuentaService> _logger;
    private readonly IClienteService _clienteService;
    private readonly ITarjetaService _tarjetaService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _database;
    private const string CacheKeyPrefix = "Cuenta:";

    public CuentaService(GeneralDbContext context, ILogger<CuentaService> logger, IProductoService productoService, IClienteService clienteService, ITarjetaService tarjetaService, IConnectionMultiplexer redis, IMemoryCache memoryCache)
    {
        _context = context;
        _logger = logger;
        _productoService = productoService;
        _clienteService = clienteService;
        _tarjetaService = tarjetaService;
        _redis = redis;
        _memoryCache = memoryCache; 
        _database = _redis.GetDatabase();
    }

    
    public async Task<PageResponse<CuentaResponse>> GetAllAsync(double? saldoMax, double? saldoMin, string? tipoCuenta, PageRequest pageRequest)
    {
        _logger.LogInformation("Buscando todos las Cuentas en la base de datos");
        int pageNumber = pageRequest.PageNumber >= 0 ? pageRequest.PageNumber : 0;
        int pageSize = pageRequest.PageSize > 0 ? pageRequest.PageSize : 10;

        var query = _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .AsQueryable();

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
            Content = content.Select(CuentaMapper.ToResponseFromEntity).ToList(),
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
        
        var clienteExiste = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteExiste == null)
        {
            _logger.LogInformation($"Cliente con guid: {guid} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guid}");
        }
        
        var query = _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .AsQueryable().Where(c => c.Cliente.Guid == guid); 
        
        var content = await query.ToListAsync();
        
        var cuentasResponses = content.Select(c => c.ToResponseFromEntity()).ToList();
        
        return cuentasResponses;
    }
    
    public async Task<IEnumerable<CuentaResponse>> GetAllMeAsync(string guid)
    {

        _logger.LogInformation($"Buscando todas las Cuentas del cliente con guid: {guid}");
        
        var clienteExiste = await _context.Clientes.FirstOrDefaultAsync(c => c.User.Guid == guid);
        if (clienteExiste == null)
        {
            _logger.LogInformation($"Cliente con guid: {guid} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guid}");
        }
        
        var query = _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .AsQueryable().Where(c => c.Cliente.User.Guid == guid); 
        
        var content = await query.ToListAsync();
        
        var cuentasResponses = content.Select(c => c.ToResponseFromEntity()).ToList();
        
        return cuentasResponses;
    }

    public async Task<CuentaResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando Cuenta con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        _logger.LogInformation($"Buscando Cuenta con guid: {guid} en cache en memoria");
        if (_memoryCache.TryGetValue(cacheKey, out Models.Cuenta? cachedCuenta))
        {
            _logger.LogDebug("Cuenta obtenida de cache en memoria");
            return cachedCuenta!.ToResponseFromModel();
        }

        _logger.LogInformation($"Buscando Cuenta con guid: {guid} en cache de redis");
        var redisCache = await _database.StringGetAsync(cacheKey);
        if (!redisCache.IsNullOrEmpty)
        {
            _logger.LogInformation("Cuenta obtenida desde Redis");
            var cuentaFromRedis = JsonSerializer.Deserialize<Models.Cuenta>(redisCache!);
            if (cuentaFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar cuenta desde Redis");
                throw new Exception("Error al deserializar cuenta desde Redis");
            }
            _memoryCache.Set(cacheKey, cuentaFromRedis, TimeSpan.FromMinutes(30));
            return cuentaFromRedis.ToResponseFromModel(); 
        }
        
        _logger.LogInformation($"Buscando Cuenta con guid: {guid} en base de datos");
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Guid == guid);
        
        if (cuentaEntity != null)
        {
            var cliente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == cuentaEntity.Cliente.Guid);
            cuentaEntity.Cliente = cliente!;
            
            _memoryCache.Set(cacheKey, cuentaEntity.ToModelFromEntity(), TimeSpan.FromMinutes(30));

            var redisValue = JsonSerializer.Serialize(cuentaEntity.ToModelFromEntity());
            await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation($"Cuenta encontrada con guid: {guid}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con guid: {guid}");
        return null;
    }

    public async Task<CuentaResponse?> GetByIbanAsync(string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban} en la base de datos");
        
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuentaEntity != null)
        {
            _logger.LogInformation($"Cuenta encontrada con iban: {iban}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }
    
    public async Task<CuentaResponse?> GetMeByIbanAsync(string guid,string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban} en la base de datos");
        
        var clienteExiste = await _context.Clientes.FirstOrDefaultAsync(c => c.User.Guid == guid);
        if (clienteExiste == null)
        {
            _logger.LogInformation($"Cliente con guid: {guid} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guid}");
        }
        
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuentaEntity != null)
        {
            if (cuentaEntity.Cliente.Guid != clienteExiste.Guid)
            {
                _logger.LogInformation($"El iban {iban} no te pertenece");
                throw new CuentaNoPertenecienteAlUsuarioException($"El iban {iban} no te pertenece");
            }
            
            _logger.LogInformation($"Cuenta encontrada con iban: {iban}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con IBAN: {iban}");
        return null;
    }

    public async Task<CuentaResponse?> GetByTarjetaGuidAsync(string tarjetaGuid)
    {
        _logger.LogInformation($"Buscando cuenta por guid de tarjeta: {tarjetaGuid} en la base de datos");
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Tarjeta != null && c.Tarjeta.Guid == tarjetaGuid);

        if (cuentaEntity != null)
        {
            _logger.LogInformation($"Cuenta encontrada con guid de tarjeta: {tarjetaGuid}");
            return cuentaEntity.ToResponseFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con guid de tarjeta: {tarjetaGuid}");
        return null;
    }

    public async Task<CuentaResponse> CreateAsync(string guid,CuentaRequest cuentaRequest)
    {
        _logger.LogInformation($"Creando cuenta nueva");

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.User.Guid == guid);

        if (cliente == null)
        {
            _logger.LogInformation($"Cliente con guid: {guid} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guid}");
        }
        
        var tipoCuenta = await _productoService.GetByTipoAsync(cuentaRequest.TipoCuenta);
        if (tipoCuenta == null)
        {
            _logger.LogError($"El tipo de cuenta {cuentaRequest.TipoCuenta} no existe en nuestro catalogo");
            throw new ProductoNotExistException($"El tipo de Cuenta {cuentaRequest.TipoCuenta} no existe en nuestro catalogo");
        }

        var tipoCuentaModel = await _productoService.GetBaseModelByGuid(tipoCuenta.Guid);
        var clienteModel = await _clienteService.GetClienteModelByGuid(cliente.Guid);
        
        if (clienteModel == null)
        {
            _logger.LogError($"El cliente {cliente.Guid} no existe ");
            throw new ClienteNotFoundException($"El cliente {cliente.Guid} no existe");
        }
        
        var cuentaEntity = new CuentaEntity
        {
            Guid = GuidGenerator.GenerarId(),
            Iban = IbanGenerator.GenerateIban(),
            Saldo = 0,
            TarjetaId = null,
            Tarjeta = null,
            ClienteId = clienteModel.Id,
            ProductoId = tipoCuentaModel!.Id,
        };
        
        cliente.Cuentas.Add(cuentaEntity);
        
        await _context.Cuentas.AddAsync(cuentaEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + cuentaEntity.Guid;
        _memoryCache.Set(cacheKey, cuentaEntity.ToModelFromEntity());
        var redisValue = JsonSerializer.Serialize(cuentaEntity.ToModelFromEntity());
        await _database.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

        var cuentaResponse = cuentaEntity.ToResponseFromEntity();
        var mensaje = $"Ha creado una cuenta con iban {cuentaEntity.Iban}";
        await WebSocketHandler.SendToCliente(cuentaEntity.Cliente.User.Username, new Notificacion { Entity = cuentaEntity.Cliente.Nombre, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});
        return cuentaResponse;
    }
    
    
    public async Task<CuentaResponse?> DeleteByGuidAsync(string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        
        var cuentaExistenteEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Guid == guid);  
        
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

        if (cuentaExistenteEntity.TarjetaId != null)
        {
            var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuentaExistenteEntity.TarjetaId);
            tarjetaExistente!.IsDeleted = true;
            tarjetaExistente.UpdatedAt = DateTime.UtcNow;
            
            _context.Tarjetas.Update(tarjetaExistente);
            await _context.SaveChangesAsync();
        }
        
        var cacheKey = CacheKeyPrefix + guid;
        _memoryCache.Remove(cacheKey);
        await _database.KeyDeleteAsync(cacheKey);
        
        _logger.LogInformation($"Cuenta borrada correctamente con guid: {guid}");
        var cuentaResponse = cuentaExistenteEntity.ToResponseFromEntity();
        var mensaje = $"Se ha eliminado su cuenta con iban {cuentaExistenteEntity.Iban}";
        await WebSocketHandler.SendToCliente(cuentaExistenteEntity.Cliente.User.Username, new Notificacion { Entity = cuentaExistenteEntity.Cliente.Nombre, Data = mensaje, Tipo = Tipo.DELETE, CreatedAt = DateTime.UtcNow.ToString()});
        return cuentaResponse;
    }
    
    public async Task<CuentaResponse?> DeleteMeAsync(string guidClient,string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.User.Guid == guidClient);

        if (cliente == null)
        {
            _logger.LogInformation($"Cliente con guid: {guidClient} no encontrado");
            throw new ClienteNotFoundException($"No se encontró el cliente con guid: {guidClient}");
        }
        
        var cuentaExistenteEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Guid == guid);  
        
        if (cuentaExistenteEntity == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            return null;
        }

        if (cuentaExistenteEntity.Cliente.Guid != guidClient)
        {
            _logger.LogInformation($"La cuenta {guid} no te pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"La cuenta {guid} no te pertenece");
            
        }

        _logger.LogInformation("Actualizando isDeleted a true");
        cuentaExistenteEntity.IsDeleted = true;
        cuentaExistenteEntity.UpdatedAt = DateTime.UtcNow;

        cliente.Cuentas.Remove(cuentaExistenteEntity);

        _context.Cuentas.Update(cuentaExistenteEntity);
        await _context.SaveChangesAsync();

        if (cuentaExistenteEntity.TarjetaId != null)
        {
            var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuentaExistenteEntity.TarjetaId);
            tarjetaExistente!.IsDeleted = true;
            tarjetaExistente.UpdatedAt = DateTime.UtcNow;
            
            _context.Tarjetas.Update(tarjetaExistente);
            await _context.SaveChangesAsync();
        }
        
        var cacheKey = CacheKeyPrefix + guid;
        _memoryCache.Remove(cacheKey);
        await _database.KeyDeleteAsync(cacheKey);
        
        _logger.LogInformation($"Cuenta borrada correctamente con guid: {guid}");
        var cuentaResponse = cuentaExistenteEntity.ToResponseFromEntity();
        var mensaje = $"Ha eliminado su cuenta con iban {cuentaExistenteEntity.Iban}";
        await WebSocketHandler.SendToCliente(cuentaExistenteEntity.Cliente.User.Username, new Notificacion { Entity = cuentaExistenteEntity.Cliente.Nombre, Data = mensaje, Tipo = Tipo.DELETE, CreatedAt = DateTime.UtcNow.ToString()});
        return cuentaResponse;
    }

    public async Task<Models.Cuenta?> GetCuentaModelByGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando cuenta con guid: {guid}");
        
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Guid == guid);
        
        if (cuentaEntity != null)
        {
            var cliente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == cuentaEntity.Cliente.Guid);
            cuentaEntity.Cliente = cliente!;
            
            _logger.LogInformation($"cuenta encontrada con guid: {guid}");
            return cuentaEntity.ToModelFromEntity();
        }

        _logger.LogInformation($"Cuenta no encontrada con guid: {guid}");
        return null;
    }
        
    public async Task<Models.Cuenta?> GetCuentaModelByIdAsync(long id)
    {
        _logger.LogInformation($"Buscando usuario con id: {id}");
        var cuentaEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (cuentaEntity != null)
        {
            var cliente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == cuentaEntity.Cliente.Guid);
            cuentaEntity.Cliente = cliente!;
            
            _logger.LogInformation($"cuenta encontrada con id: {id}");
            return cuentaEntity.ToModelFromEntity(); 
        }

        _logger.LogInformation($"Cuenta no encontrada con id: {id}");
        return null;
    }
    // hacemos un GetAllForStorage sin filtrado ni paginacion que devuelve un model
    public async Task<List<Models.Cuenta>> GetAllForStorage()
    {
        _logger.LogInformation("Buscando todas las cuentas");

        var cuentasEntity = await _context.Cuentas
            .Include(c => c.Tarjeta)
            .Include(c => c.Cliente)
            .Include(c => c.Producto)
            .ToListAsync();
        
        if (cuentasEntity.Count == 0)
        {
            _logger.LogInformation("Cuentas encontradas");
            foreach (var cuentaEntity in cuentasEntity)
            {
                var cliente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == cuentaEntity.Cliente.Guid);
                cuentaEntity.Cliente = cliente!;
            }
            return cuentasEntity.Select(c => c.ToModelFromEntity()).ToList();
        }

        _logger.LogInformation("No se encontraron cuentas");
        return new List<Models.Cuenta>();
    }
}