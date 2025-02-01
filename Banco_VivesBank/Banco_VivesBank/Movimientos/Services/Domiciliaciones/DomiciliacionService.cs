using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Websockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Banco_VivesBank.Movimientos.Services.Domiciliaciones;

public class DomiciliacionService : IDomiciliacionService
{
    private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private readonly ILogger<DomiciliacionService> _logger;
    private readonly IClienteService _clienteService;
    private readonly ICuentaService _cuentaService;
    private readonly GeneralDbContext _context;
    private readonly IDatabase _redisDatabase;
    private readonly IMemoryCache _memoryCache;
    private readonly IMovimientoService _movimientoService;
    private const string CacheKeyPrefix = "Domiciliaciones:";

    public DomiciliacionService(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<DomiciliacionService> logger, IClienteService clienteService, ICuentaService cuentaService, GeneralDbContext context, IConnectionMultiplexer redis, IMemoryCache memoryCache, IMovimientoService movimientoService)
    {
        _logger = logger;
        _clienteService = clienteService;
        _cuentaService = cuentaService;
        _context = context;
        _redisDatabase = redis.GetDatabase();
        _memoryCache = memoryCache;
        _movimientoService = movimientoService;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
    }
    
    public async Task<IEnumerable<DomiciliacionResponse>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todas las domiciliaciones en la base de datos");
        var domiciliaciones = await _domiciliacionCollection.Find(_ => true).ToListAsync();
        return domiciliaciones.Select(mov => mov.ToResponseFromModel());
    }

    public async Task<DomiciliacionResponse?> GetByGuidAsync(string domiciliacionGuid)
    {
        _logger.LogInformation($"Buscando domiciliacion con guid {domiciliacionGuid}");
        
        var cacheKey = CacheKeyPrefix + domiciliacionGuid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Domiciliacion? memoryCacheDom))
        {
            _logger.LogInformation("Domiciliación obtenida desde la memoria caché");
            return memoryCacheDom.ToResponseFromModel();
        }

        // Intentar obtener desde la caché de Redis
        var redisCacheValue = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheValue.IsNullOrEmpty)
        {
            _logger.LogInformation("Domiciliación obtenida desde Redis");
            var domFromRedis = JsonSerializer.Deserialize<Domiciliacion>(redisCacheValue!);
            if (domFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar domiciliación desde Redis");
                throw new MovimientoDeserialiceException("Error al deserializar domiciliación desde Redis");
            }

            _memoryCache.Set(cacheKey, domFromRedis, TimeSpan.FromMinutes(30));
            return domFromRedis.ToResponseFromModel();
        }
        
        _logger.LogInformation($"Buscando domiciliación con guid {domiciliacionGuid} en la base de datos");
        var domiciliacion = await _domiciliacionCollection.Find(dom => dom.Guid == domiciliacionGuid).FirstOrDefaultAsync();
        if (domiciliacion != null)
        {
            _logger.LogInformation($"Encontrada domiciliacion con guid {domiciliacionGuid} en la base de datos");
            return domiciliacion.ToResponseFromModel();
        }
        
        _logger.LogInformation($"No se ha encontrado ninguna domiciliación con guid {domiciliacionGuid}");
        return null;
    }

    public async Task<IEnumerable<DomiciliacionResponse>> GetByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Buscando todas las domiciliaciones del cliente con guid: {clienteGuid}");
        var domiciliaciones = await _domiciliacionCollection.Find(dom => dom.ClienteGuid == clienteGuid).ToListAsync();
        return domiciliaciones.Select(mov => mov.ToResponseFromModel());
    }

    public async Task<IEnumerable<DomiciliacionResponse>> GetMyDomiciliaciones(User.Models.User userAuth)
    {
        _logger.LogInformation($"Buscando todas las domiciliaciones del cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        var domiciliaciones = await _domiciliacionCollection.Find(dom => dom.ClienteGuid == cliente!.Guid).ToListAsync();
        return domiciliaciones.Select(mov => mov.ToResponseFromModel());
    }

    public async Task<DomiciliacionResponse> CreateAsync(User.Models.User userAuth, DomiciliacionRequest domiciliacionRequest)
    {
        _logger.LogInformation("Creando domiciliación");
        
        _logger.LogInformation($"Buscando si existe el autenticado");
        var me = await _clienteService.GetMeAsync(userAuth);
        if (me == null)
        {
            _logger.LogWarning($"No se ha encontrado ningún cliente autenticado");
            throw new ClienteNotFoundException($"No se ha encontrado ningún cliente autenticado");
        }
        var cliente = await _clienteService.GetClienteModelByGuid(me.Guid);
        
        _logger.LogInformation($"Validando existencia de la cuenta con iban: {domiciliacionRequest.IbanCliente}");
        var cuenta = await _cuentaService.GetByIbanAsync(domiciliacionRequest.IbanCliente);
        if (cuenta == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
        }
        
        _logger.LogInformation($"Comprobando que el iban con guid: {domiciliacionRequest.IbanCliente} pertenezca a alguna de las cuentas del cliente con guid: {cliente!.Guid}");
        if (cuenta.ClienteGuid != cliente.Guid)
        {
            _logger.LogWarning($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {cliente.Guid}");
            throw new CuentaIbanException($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {cliente.Guid}");
        }
        
        _logger.LogInformation($"Validando saldo suficiente respecto al importe de: {domiciliacionRequest.Importe} €");
        if (cuenta.Saldo < domiciliacionRequest.Importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €");
            throw new SaldoCuentaInsuficientException($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €");
        }
        
        _logger.LogInformation("Validando periodicidad valida");
        if (!Enum.TryParse(domiciliacionRequest.Periodicidad, out Periodicidad periodicidad))
        {
            _logger.LogWarning($"Periodicidad no válida: {domiciliacionRequest.Periodicidad}");
            throw new PeriodicidadNotValidException($"Periodicidad no válida: {domiciliacionRequest.Periodicidad}");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();

            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo -= domiciliacionRequest.Importe;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuenta.Guid}");
            throw new MovimientoTransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuenta.Guid}\n\n{ex.Message}");
        }

        Domiciliacion domiciliacion = new Domiciliacion
        {
            ClienteGuid = cliente.Guid,
            Acreedor = domiciliacionRequest.Acreedor,
            IbanEmpresa = domiciliacionRequest.IbanEmpresa,
            IbanCliente = domiciliacionRequest.IbanCliente,
            Importe = domiciliacionRequest.Importe,
            Periodicidad = (Periodicidad)Enum.Parse(typeof(Periodicidad), domiciliacionRequest.Periodicidad),
            Activa = domiciliacionRequest.Activa
        };
        
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);
        _logger.LogInformation("Domiciliación realizada con éxito");
        
        var cacheKey = CacheKeyPrefix + domiciliacion.Guid;
        
        // Guardar en las cachés 
        var serializedDom = JsonSerializer.Serialize(domiciliacion);
        _memoryCache.Set(cacheKey, domiciliacion, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedDom, TimeSpan.FromMinutes(30));

        MovimientoRequest movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = cliente.Guid,
            Domiciliacion = domiciliacion,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = null
        };
        
        await _movimientoService.CreateAsync(movimientoRequest);
        _logger.LogInformation("Movimiento del pago inicial de la domiciliación generado con éxito");
        
        var domiciliacionResponse = domiciliacion.ToResponseFromModel();
        var mensaje = $"Se ha creado una domiciliacion al acreedor {domiciliacion.Acreedor}, con un importe de {domiciliacion.Importe}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});
        return domiciliacionResponse!;
    }

    public async Task<DomiciliacionResponse?> DesactivateDomiciliacionAsync(string domiciliacionGuid)
    {
        _logger.LogInformation($"Desactivando domiciliación con guid {domiciliacionGuid}");
        
        var cacheKey = CacheKeyPrefix + domiciliacionGuid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Domiciliacion? memoryCacheDom))
        {
            _logger.LogInformation("Domiciliación obtenida desde la memoria caché");
            return memoryCacheDom.ToResponseFromModel();
        }

        // Intentar obtener desde la caché de Redis
        var redisCacheValue = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheValue.IsNullOrEmpty)
        {
            _logger.LogInformation("Domiciliación obtenida desde Redis");
            var domFromRedis = JsonSerializer.Deserialize<Domiciliacion>(redisCacheValue!);
            if (domFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar domiciliación desde Redis");
                throw new MovimientoDeserialiceException("Error al deserializar domiciliación desde Redis");
            }

            _memoryCache.Set(cacheKey, domFromRedis, TimeSpan.FromMinutes(30));
            return domFromRedis.ToResponseFromModel();
        }

        // Intentar obtener de la base de datos
        var domiciliacion = await _domiciliacionCollection.Find(dom => dom.Guid == domiciliacionGuid).FirstOrDefaultAsync();

        if (domiciliacion == null)
        {
            _logger.LogInformation($"No se ha encontrado la domiciliacion con guid {domiciliacionGuid}");
            return null;
        }
        
        domiciliacion.Activa = false;
        await _domiciliacionCollection.ReplaceOneAsync(m => m.Guid == domiciliacionGuid, domiciliacion);
        
        // Eliminar de la memoria caché y de redis
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        // Añadimos con los datos nuevos
        var serializedDom = JsonSerializer.Serialize(domiciliacion);
        _memoryCache.Set(cacheKey, domiciliacion, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedDom, TimeSpan.FromMinutes(30));
        
        var cliente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == domiciliacion.ClienteGuid);
        
        _logger.LogInformation("Domiciliación desactivada con éxito");
        var domiciliacionResponse = domiciliacion.ToResponseFromModel();
        var mensaje = $"Se ha desactivado una domiciliacion al acreedor {domiciliacion.Acreedor}";
        await WebSocketHandler.SendToCliente(cliente.User.Username, new Notificacion{Entity = cliente.Nombre, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});
        return domiciliacionResponse!;
    }
    
    public async Task<DomiciliacionResponse?> DesactivateMyDomiciliacionAsync(User.Models.User userAuth, string domiciliacionGuid)
    {
        _logger.LogInformation($"Desactivando domiciliación con guid {domiciliacionGuid}");
        
        var cacheKey = CacheKeyPrefix + domiciliacionGuid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Domiciliacion? memoryCacheDom))
        {
            _logger.LogInformation("Domiciliación obtenida desde la memoria caché");
            return memoryCacheDom.ToResponseFromModel();
        }

        // Intentar obtener desde la caché de Redis
        var redisCacheValue = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheValue.IsNullOrEmpty)
        {
            _logger.LogInformation("Domiciliación obtenida desde Redis");
            var domFromRedis = JsonSerializer.Deserialize<Domiciliacion>(redisCacheValue!);
            if (domFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar domiciliación desde Redis");
                throw new MovimientoDeserialiceException("Error al deserializar domiciliación desde Redis");
            }

            _memoryCache.Set(cacheKey, domFromRedis, TimeSpan.FromMinutes(30));
            return domFromRedis.ToResponseFromModel();
        }

        // Intentar obtener de la base de datos
        var domiciliacion = await _domiciliacionCollection.Find(dom => dom.Guid == domiciliacionGuid).FirstOrDefaultAsync();

        if (domiciliacion == null)
        {
            _logger.LogInformation($"No se ha encontrado la domiciliacion con guid {domiciliacionGuid}");
            return null;
        }
        
        _logger.LogInformation($"Validando que la domiciliacion con guid {domiciliacion.Guid} pertenezca al cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        if (domiciliacion.ClienteGuid != cliente!.Guid)
        {
            _logger.LogWarning($"La domiciliación con guid {domiciliacion.Guid} no pertenece al cliente autenticado con guid {cliente.Guid} y no puede ser desactivada");
            throw new MovimientoNoPertenecienteAlUsuarioAutenticadoException($"La domiciliación con guid {domiciliacion.Guid} no pertenece al cliente autenticado con guid {cliente.Guid} y no puede ser desactivada");
        }
        
        domiciliacion.Activa = false;
        await _domiciliacionCollection.ReplaceOneAsync(m => m.Guid == domiciliacionGuid, domiciliacion);
        
        // Eliminar de la memoria caché y de redis
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        // Añadimos con los datos nuevos
        var serializedDom = JsonSerializer.Serialize(domiciliacion);
        _memoryCache.Set(cacheKey, domiciliacion, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedDom, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("Domiciliación desactivada con éxito");
        var domiciliacionResponse = domiciliacion.ToResponseFromModel();
        var mensaje = $"Ha desactivado una domiciliacion al acreedor {domiciliacion.Acreedor}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});
        return domiciliacionResponse!;
    }
}