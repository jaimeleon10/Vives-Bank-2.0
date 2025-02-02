using System.Text.Json;
using System.Transactions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Websockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;
using CuentaInvalidaException = Banco_VivesBank.Storage.Pdf.Exception;

namespace Banco_VivesBank.Movimientos.Services.Movimientos;

public class MovimientoService : IMovimientoService
{
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly ILogger<MovimientoService> _logger;
    private readonly IClienteService _clienteService;
    private readonly ICuentaService _cuentaService;
    private readonly ITarjetaService _tarjetaService;
    private readonly GeneralDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _redisDatabase;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKeyPrefix = "Movimientos:";
    
    public MovimientoService(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<MovimientoService> logger, IClienteService clienteService, ICuentaService cuentaService, GeneralDbContext context, ITarjetaService tarjetaService, IConnectionMultiplexer redis, IMemoryCache memoryCache)
    {
        _logger = logger;
        _clienteService = clienteService;
        _cuentaService = cuentaService;
        _context = context;
        _tarjetaService = tarjetaService;
        _redis = redis;
        _redisDatabase = _redis.GetDatabase();
        _memoryCache = memoryCache;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.MovimientosCollectionName);
    }
    
    public async Task<IEnumerable<MovimientoResponse>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todos los movimientos en la base de datos");
        var movimientos = await _movimientoCollection.Find(_ => true).ToListAsync();
        return movimientos.Select(mov => mov.ToResponseFromModel(
            mov.Domiciliacion.ToResponseFromModel(),
            mov.IngresoNomina.ToResponseFromModel(),
            mov.PagoConTarjeta.ToResponseFromModel(),
            mov.Transferencia.ToResponseFromModel()
        ));
    }

    public async Task<MovimientoResponse?> GetByGuidAsync(string guid)
    {
        _logger.LogInformation($"Buscando movimiento con guid: {guid}");
        
        var cacheKey = CacheKeyPrefix + guid;
        
        // Intentar obtener desde la memoria caché
        if (_memoryCache.TryGetValue(cacheKey, out Movimiento? memoryCacheMov))
        {
            _logger.LogInformation("Movimiento obtenido desde la memoria caché");
            return memoryCacheMov!.ToResponseFromModel(
                memoryCacheMov?.Domiciliacion.ToResponseFromModel(), 
                memoryCacheMov?.IngresoNomina.ToResponseFromModel(), 
                memoryCacheMov?.PagoConTarjeta.ToResponseFromModel(), 
                memoryCacheMov?.Transferencia.ToResponseFromModel());
        }

        // Intentar obtener desde la caché de Redis
        var redisCacheValue = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheValue.IsNullOrEmpty)
        {
            _logger.LogInformation("Movimiento obtenido desde Redis");
            var movFromRedis = JsonSerializer.Deserialize<Movimiento>(redisCacheValue!);
            if (movFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar movimiento desde Redis");
                throw new MovimientoDeserialiceException("Error al deserializar movimiento desde Redis");
            }

            _memoryCache.Set(cacheKey, movFromRedis, TimeSpan.FromMinutes(30));
            return movFromRedis.ToResponseFromModel(
                memoryCacheMov?.Domiciliacion.ToResponseFromModel(), 
                memoryCacheMov?.IngresoNomina.ToResponseFromModel(), 
                memoryCacheMov?.PagoConTarjeta.ToResponseFromModel(), 
                memoryCacheMov?.Transferencia.ToResponseFromModel());
        }
        
        // Consultar la base de datos
        var movimiento = await _movimientoCollection.Find(m => m.Guid == guid).FirstOrDefaultAsync();
        
        if (movimiento != null)
        {
            _logger.LogInformation($"Movimiento con guid: {guid} encontrado en base de datos.");
            return movimiento.ToResponseFromModel(
                movimiento.Domiciliacion.ToResponseFromModel(),
                movimiento.IngresoNomina.ToResponseFromModel(),
                movimiento.PagoConTarjeta.ToResponseFromModel(),
                movimiento.Transferencia.ToResponseFromModel());
        }
        
        _logger.LogWarning($"Movimiento no encontrado con guid: {guid}");
        return null;
    }

    public async Task<IEnumerable<MovimientoResponse>> GetByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Buscando todos los movimientos del cliente con guid: {clienteGuid}");
        var movimientos = await _movimientoCollection.Find(mov => mov.ClienteGuid == clienteGuid).ToListAsync();
        return movimientos.Select(mov => mov.ToResponseFromModel(
            mov.Domiciliacion.ToResponseFromModel(),
            mov.IngresoNomina.ToResponseFromModel(),
            mov.PagoConTarjeta.ToResponseFromModel(),
            mov.Transferencia.ToResponseFromModel()
        ));
    }

    public async Task<IEnumerable<MovimientoResponse>> GetMyMovimientos(User.Models.User userAuth)
    {
        _logger.LogInformation($"Buscando todos los movimientos del cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        var movimientos = await _movimientoCollection.Find(mov => mov.ClienteGuid == cliente!.Guid).ToListAsync();
        return movimientos.Select(mov => mov.ToResponseFromModel(
            mov.Domiciliacion.ToResponseFromModel(),
            mov.IngresoNomina.ToResponseFromModel(),
            mov.PagoConTarjeta.ToResponseFromModel(),
            mov.Transferencia.ToResponseFromModel()
        ));
    }

    public async Task CreateAsync(MovimientoRequest movimientoRequest)
    {
        _logger.LogInformation("Guardando movimiento");

        Movimiento nuevoMovimiento = new Movimiento
        {
            ClienteGuid = movimientoRequest.ClienteGuid,
            Domiciliacion = movimientoRequest.Domiciliacion,
            IngresoNomina = movimientoRequest.IngresoNomina,
            PagoConTarjeta = movimientoRequest.PagoConTarjeta,
            Transferencia = movimientoRequest.Transferencia
        };
        
        var cacheKey = CacheKeyPrefix + nuevoMovimiento.Guid;

        // Guardar en las cachés 
        var serializedMov = JsonSerializer.Serialize(nuevoMovimiento);
        _memoryCache.Set(cacheKey, nuevoMovimiento, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedMov, TimeSpan.FromMinutes(30));
        
        await _movimientoCollection.InsertOneAsync(nuevoMovimiento);
        _logger.LogInformation("Movimiento guardado con éxito");
    }

    public async Task<IngresoNominaResponse> CreateIngresoNominaAsync(User.Models.User userAuth, IngresoNominaRequest ingresoNominaRequest)
    {
        _logger.LogInformation("Creando ingreso de nómina");
        
        _logger.LogInformation($"Validando existencia de la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
        var cuenta = await _cuentaService.GetByIbanAsync(ingresoNominaRequest.IbanCliente);
        if (cuenta == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
        }
        
        _logger.LogInformation($"Buscando cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        
        _logger.LogInformation($"Validando pertenencia de la cuenta con guid {cuenta.Guid} al cliente con guid {cliente!.Guid}");
        if (cuenta.ClienteGuid != cliente!.Guid)
        {
            _logger.LogWarning($"La cuenta con guid {cuenta.Guid} no pertenece al cliente autenticado con guid {cliente.Guid}");
            throw new CuentaNoPertenecienteAlUsuarioException($"La cuenta con guid {cuenta.Guid} no pertenece al cliente con guid {cliente.Guid}");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();
            
            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo += ingresoNominaRequest.Importe;

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
        
        IngresoNomina ingresoNomina = new IngresoNomina
        {
            NombreEmpresa = ingresoNominaRequest.NombreEmpresa,
            CifEmpresa = ingresoNominaRequest.CifEmpresa,
            IbanEmpresa = ingresoNominaRequest.IbanEmpresa,
            IbanCliente = ingresoNominaRequest.IbanCliente,
            Importe = ingresoNominaRequest.Importe
        };
        
        _logger.LogInformation("Ingreso de nómina realizado con éxito");

        MovimientoRequest movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = cuenta.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = ingresoNomina,
            PagoConTarjeta = null,
            Transferencia = null
        };
        
        await CreateAsync(movimientoRequest);
        var ingresoResponse = ingresoNomina.ToResponseFromModel();
        var mensaje =
            $"Se ha realizado el ingreso de la nomina con un importe de  {ingresoNomina.Importe} a la cuenta con iban {ingresoNomina.IbanCliente} del cliente {cliente.Nombre} {cliente.Apellidos}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});

        return ingresoResponse!;
    }

    public async Task<PagoConTarjetaResponse> CreatePagoConTarjetaAsync(User.Models.User userAuth, PagoConTarjetaRequest pagoConTarjetaRequest)
    {
        _logger.LogInformation("Creando pago con tarjeta");
        
        _logger.LogInformation($"Validando existencia de la tarjeta con número: {pagoConTarjetaRequest.NumeroTarjeta}");
        var tarjeta = await _tarjetaService.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta);
        if (tarjeta == null)
        {
            _logger.LogWarning($"No se ha encontrado la tarjeta con número: {pagoConTarjetaRequest.NumeroTarjeta}");
            throw new TarjetaNotFoundException($"No se ha encontrado la tarjeta con número: {pagoConTarjetaRequest.NumeroTarjeta}");
        }
        
        _logger.LogInformation($"Buscando cuenta con guid de tarjeta: {tarjeta.Guid}");
        var cuenta = await _cuentaService.GetByTarjetaGuidAsync(tarjeta.Guid);
        if (cuenta == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta asociada a la tarjeta con guid: {tarjeta.Guid}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta asociada a la tarjeta con guid: {tarjeta.Guid}");
        }
        
        _logger.LogInformation("Buscando cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        
        _logger.LogInformation($"Validando pertenencia de la cuenta con guid {cuenta.Guid} al cliente con guid {cliente!.Guid}");
        if (cuenta.ClienteGuid != cliente!.Guid)
        {
            _logger.LogWarning($"La tarjeta con guid {tarjeta.Guid} perteneciente a la cuenta con guid {cuenta.Guid} no pertenece al cliente autenticado con guid {cliente.Guid}");
            throw new CuentaNoPertenecienteAlUsuarioException($"La tarjeta con guid {tarjeta.Guid} perteneciente a la cuenta con guid {cuenta.Guid} no pertenece al cliente con guid {cliente.Guid}");
        }
        
        _logger.LogInformation($"Validando saldo suficiente en la cuenta con guid: {cuenta.Guid} perteneciente a la tarjeta con guid: {tarjeta.Guid}");
        if (cuenta.Saldo < pagoConTarjetaRequest.Importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {pagoConTarjetaRequest.Importe} €");
            throw new SaldoCuentaInsuficientException($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {pagoConTarjetaRequest.Importe} €");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();

            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo -= pagoConTarjetaRequest.Importe;

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
            throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuenta.Guid}\n\n{ex.Message}");
        }
        
        PagoConTarjeta pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = pagoConTarjetaRequest.NombreComercio,
            Importe = pagoConTarjetaRequest.Importe,
            NumeroTarjeta = pagoConTarjetaRequest.NumeroTarjeta
        };
        
        _logger.LogInformation("Pago con tarjeta realizado con éxito");

        MovimientoRequest movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = cuenta.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = pagoConTarjeta,
            Transferencia = null
        };
        
        await CreateAsync(movimientoRequest);
        var pagoResponse = pagoConTarjeta.ToResponseFromModel();
        var mensaje =
            $"Se ha realizado el pago con la tarjeta con numero {pagoConTarjeta.NumeroTarjeta}, con un importe de {pagoConTarjeta.Importe} a la cuenta con iban {cuenta.Iban} del cliente {cliente.Nombre} {cliente.Apellidos}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensaje, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});

        return pagoResponse!;
    }

    public async Task<TransferenciaResponse> CreateTransferenciaAsync(User.Models.User userAuth, TransferenciaRequest transferenciaRequest)
    {
        _logger.LogInformation("Creando transferencia");
        
        _logger.LogInformation("Validando que la cuenta de origen y destino sean distintas");
        if (transferenciaRequest.IbanOrigen == transferenciaRequest.IbanDestino)
        {
            _logger.LogWarning("Las cuentas de origen y destino deben ser distintas");
            throw new Producto.Cuenta.Exceptions.CuentaInvalidaException("Las cuentas de origen y destino deben ser distintas");
        }
        
        _logger.LogInformation("Validando existencia de la cuenta de origen");
        var cuentaOrigen = await _cuentaService.GetByIbanAsync(transferenciaRequest.IbanOrigen);
        if (cuentaOrigen == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {transferenciaRequest.IbanOrigen}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {transferenciaRequest.IbanOrigen}");
        }
        
        _logger.LogInformation($"Buscando cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        
        _logger.LogInformation($"Validando pertenencia de la cuenta de origen con guid {cuentaOrigen.Guid} al cliente con guid {cliente!.Guid}");
        if (cuentaOrigen.ClienteGuid != cliente.Guid)
        {
            _logger.LogWarning($"La cuenta de origen con guid {cuentaOrigen.Guid} no pertenece al cliente autenticado con guid {cliente.Guid}");
            throw new CuentaNoPertenecienteAlUsuarioException($"La cuenta de origen con guid {cuentaOrigen.Guid} no pertenece al cliente con guid {cliente.Guid}");
        }
        
        _logger.LogInformation($"Validando saldo suficiente en la cuenta de origen con guid {cuentaOrigen.Guid}");
        if (cuentaOrigen.Saldo < transferenciaRequest.Importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta de origen con guid: {cuentaOrigen.Guid} respecto al importe de {transferenciaRequest.Importe} €");
            throw new SaldoCuentaInsuficientException($"Saldo insuficiente en la cuenta de origen con guid: {cuentaOrigen.Guid} respecto al importe de {transferenciaRequest.Importe} ���");
        }
        
        _logger.LogInformation("Validando existencia de la cuenta de destino en caso de pertenecer a VivesBank");
        var cuentaDestino = await _cuentaService.GetByIbanAsync(transferenciaRequest.IbanDestino);
        
        if (cuentaDestino != null)
        {
            await using var transactionCuentaDestino = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaUpdateDestino = await _context.Cuentas.Where(c => c.Guid == cuentaDestino.Guid).FirstOrDefaultAsync();

                if (cuentaUpdateDestino != null)
                {
                    cuentaUpdateDestino.Saldo += transferenciaRequest.Importe;

                    await _context.SaveChangesAsync();
                    await transactionCuentaDestino.CommitAsync();
                }
                else
                {
                    await transactionCuentaDestino.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transactionCuentaDestino.RollbackAsync();
                _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuentaDestino.Guid}");
                throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaDestino.Guid}\n\n{ex.Message}");
            }
        }
        
        await using var transactionCuentaOrigen = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdateOrigen = await _context.Cuentas.Where(c => c.Guid == cuentaOrigen.Guid).FirstOrDefaultAsync();

            if (cuentaUpdateOrigen != null)
            {
                cuentaUpdateOrigen.Saldo -= transferenciaRequest.Importe;

                await _context.SaveChangesAsync();
                await transactionCuentaOrigen.CommitAsync();
            }
            else
            {
                await transactionCuentaOrigen.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            await transactionCuentaOrigen.RollbackAsync();
            _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuentaOrigen.Guid}");
            throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaOrigen.Guid}\n\n{ex.Message}");
        }
        
        _logger.LogInformation("Transferencia realizada con éxito");

        if (cuentaDestino != null)
        {
            Transferencia transferenciaDestino = new Transferencia
            {
                ClienteOrigen = cliente.Nombre + " " + cliente.Apellidos,
                IbanOrigen = transferenciaRequest.IbanOrigen,
                NombreBeneficiario = transferenciaRequest.NombreBeneficiario,
                IbanDestino = transferenciaRequest.IbanDestino,
                Importe = transferenciaRequest.Importe
            };
            
            MovimientoRequest movimientoRequestDestino = new MovimientoRequest
            {
                ClienteGuid = cuentaDestino.ClienteGuid,
                Domiciliacion = null,
                IngresoNomina = null,
                PagoConTarjeta = null,
                Transferencia = transferenciaDestino
            };
            
            await CreateAsync(movimientoRequestDestino);
            _logger.LogInformation("Movimiento de la transferencia en la cuenta de destino generado con éxito");
        }

        Transferencia transferenciaOrigen = new Transferencia
        {
            ClienteOrigen = cliente.Nombre + " " + cliente.Apellidos,
            IbanOrigen = transferenciaRequest.IbanOrigen,
            NombreBeneficiario = transferenciaRequest.NombreBeneficiario,
            IbanDestino = transferenciaRequest.IbanDestino,
            Importe = -transferenciaRequest.Importe
        };
        
        MovimientoRequest movimientoRequestOrigen = new MovimientoRequest
        {
            ClienteGuid = cuentaOrigen.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = transferenciaOrigen
        };
        
        await CreateAsync(movimientoRequestOrigen);
        _logger.LogInformation("Movimiento de la transferencia en la cuenta de origen generado con éxito");
        var transferenciaResponse = transferenciaOrigen.ToResponseFromModel();
        var destino = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == cuentaDestino.ClienteGuid);
        var mensajeOrigen = $"Has realizado una transferencia al cliente {transferenciaRequest.NombreBeneficiario}, con un importe de {transferenciaRequest.Importe}";
        var mensajeDestino = $"El cliente {transferenciaResponse.ClienteOrigen} ha realizado una transferencia a tu cuenta, con un importe de {transferenciaRequest.Importe}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensajeOrigen, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});
        await WebSocketHandler.SendToCliente(destino.User.Username, new Notificacion{Entity = destino.User.Username, Data = mensajeDestino, Tipo = Tipo.CREATE, CreatedAt = DateTime.UtcNow.ToString()});

        return transferenciaResponse!;
    }

    public async Task<TransferenciaResponse> RevocarTransferenciaAsync(User.Models.User userAuth, string movimientoGuid)
    {
        _logger.LogInformation("Revocando transferencia");
        
        _logger.LogInformation($"Buscando movimiento de la transferencia con guid: {movimientoGuid}");
        var movimiento = await _movimientoCollection.Find(m => m.Guid == movimientoGuid).FirstOrDefaultAsync();
        if (movimiento == null || movimiento.Transferencia == null)
        {
            _logger.LogWarning($"No existe el movimiento con guid: {movimientoGuid} o no es un movimiento de transferencia");
            throw new MovimientoNotFoundException($"No existe el movimiento con guid: {movimientoGuid} o no es un movimiento de transferencia");
        }
        
        _logger.LogInformation($"Buscando cliente autenticado");
        var cliente = await _clienteService.GetMeAsync(userAuth);
        
        _logger.LogInformation($"Validando que el movimiento de la transferencia a revocar pertenezca al usuario que lo solicita");
        if (movimiento.ClienteGuid != cliente!.Guid)
        {
            _logger.LogWarning($"El movimiento de la transferencia con guid: {movimientoGuid} no pertenece al usuario que lo solicita");
            throw new MovimientoNoPertenecienteAlUsuarioAutenticadoException($"El movimiento de la transferencia con guid: {movimientoGuid} no pertenece al usuario que lo solicita");
        }
        
        _logger.LogInformation("Validando que la transferencia a revocar sea una transferencia recibida");
        if (movimiento.Transferencia.Importe < 0)
        {
            _logger.LogWarning($"La transferencia con guid de movimiento: {movimientoGuid} no es una transferencia recibida");
            throw new TransferenciaEmitidaException($"La transferencia con guid de movimiento: {movimientoGuid} no es una transferencia recibida");
        }
        
        _logger.LogInformation($"Validando que la transferencia no haya sido revocada previamente");
        if (movimiento.Transferencia.Revocada)
        {
            _logger.LogWarning($"La transferencia con guid de movimiento: {movimientoGuid} ya ha sido revocada previamente");
            throw new TransferenciaRevocadaException($"La transferencia con guid de movimiento: {movimientoGuid} ya ha sido revocada previamente");
        }
        
        _logger.LogInformation("Validando existencia de la nueva cuenta origen de la transferencia");
        // Cuenta destino "old" de la transferencia que se va a revocar
        var cuentaOrigenNew = await _cuentaService.GetByIbanAsync(movimiento.Transferencia.IbanDestino);
        if (cuentaOrigenNew == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {movimiento.Transferencia.IbanDestino}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {movimiento.Transferencia.IbanDestino}");
        }
        
        _logger.LogInformation($"Validando pertenencia de la nueva cuenta de origen con guid {cuentaOrigenNew.Guid} al cliente con guid {cliente!.Guid}");
        if (cuentaOrigenNew.ClienteGuid != cliente!.Guid)
        {
            _logger.LogWarning($"La nueva cuenta de origen con guid {cuentaOrigenNew.Guid} no pertenece al cliente autenticado con guid {cliente.Guid}");
            throw new CuentaNoPertenecienteAlUsuarioException($"La nueva cuenta de origen con guid {cuentaOrigenNew.Guid} no pertenece al cliente con guid {cliente.Guid}");
        }

        if (cuentaOrigenNew.Saldo < movimiento.Transferencia.Importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuentaOrigenNew.Guid} respecto al importe de {movimiento.Transferencia.Importe} €");
            throw new SaldoCuentaInsuficientException($"Saldo insuficiente en la cuenta con guid: {cuentaOrigenNew.Guid} respecto al importe de {movimiento.Transferencia.Importe} €");
        }
        
        _logger.LogWarning($"{movimiento.Transferencia.Importe}");
        
        await using var transactionCuentaOrigenNew = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdateOrigenNew = await _context.Cuentas.Where(c => c.Guid == cuentaOrigenNew.Guid).FirstOrDefaultAsync();

            if (cuentaUpdateOrigenNew != null)
            {
                cuentaUpdateOrigenNew.Saldo -= movimiento.Transferencia.Importe;

                await _context.SaveChangesAsync();
                await transactionCuentaOrigenNew.CommitAsync();
            }
            else
            {
                await transactionCuentaOrigenNew.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            await transactionCuentaOrigenNew.RollbackAsync();
            _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuentaOrigenNew.Guid}");
            throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaOrigenNew.Guid}\n\n{ex.Message}");
        }
        
        _logger.LogInformation("Validando existencia de la nueva cuenta destino en caso de pertenecer a VivesBank");
        // Cuenta origen "old" de la transferencia que se va a revocar
        var cuentaDestinoNew = await _cuentaService.GetByIbanAsync(movimiento.Transferencia.IbanOrigen);
        if (cuentaDestinoNew != null)
        {
            await using var transactionCuentaDestinoNew = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaUpdateDestinoNew = await _context.Cuentas.Where(c => c.Guid == cuentaDestinoNew.Guid).FirstOrDefaultAsync();

                if (cuentaUpdateDestinoNew != null)
                {
                    cuentaUpdateDestinoNew.Saldo += movimiento.Transferencia.Importe;

                    await _context.SaveChangesAsync();
                    await transactionCuentaDestinoNew.CommitAsync();
                }
                else
                {
                    await transactionCuentaDestinoNew.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transactionCuentaDestinoNew.RollbackAsync();
                _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuentaDestinoNew.Guid}");
                throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaDestinoNew.Guid}\n\n{ex.Message}");
            }
        }

        if (cuentaDestinoNew != null)
        {
            Transferencia transferenciaDestinoNew = new Transferencia
            {
                ClienteOrigen = cliente.Nombre + " " + cliente.Apellidos,
                IbanOrigen = movimiento.Transferencia.IbanOrigen,
                NombreBeneficiario = movimiento.Transferencia.ClienteOrigen,
                IbanDestino = movimiento.Transferencia.IbanDestino,
                Importe = movimiento.Transferencia.Importe
            };
            
            MovimientoRequest movimientoRequestOrigen = new MovimientoRequest
            {
                ClienteGuid = cuentaDestinoNew.ClienteGuid,
                Domiciliacion = null,
                IngresoNomina = null,
                PagoConTarjeta = null,
                Transferencia = transferenciaDestinoNew
            };
            
            await CreateAsync(movimientoRequestOrigen);
            _logger.LogInformation("Movimiento de la revocación de transferencia en la cuenta de origen generado con éxito");
        }
        
        Transferencia transferenciaOrigenNew = new Transferencia
        {
            ClienteOrigen = cliente.Nombre + " " + cliente.Apellidos,
            IbanOrigen = movimiento.Transferencia.IbanOrigen,
            NombreBeneficiario = movimiento.Transferencia.ClienteOrigen,
            IbanDestino = movimiento.Transferencia.IbanDestino,
            Importe = -movimiento.Transferencia.Importe
        };

        movimiento.Transferencia.Revocada = true;
        await _movimientoCollection.ReplaceOneAsync(m => m.Guid == movimientoGuid, movimiento);
        _logger.LogInformation("Transferencia revocada con éxito");

        MovimientoRequest movimientoRequestDestino = new MovimientoRequest
        {
            ClienteGuid = cuentaOrigenNew.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = transferenciaOrigenNew
        };
        
        await CreateAsync(movimientoRequestDestino);
        _logger.LogInformation("Movimiento de la revocación de transferencia en la cuenta de destino generado con éxito");
        var transferenciaResponse = transferenciaOrigenNew.ToResponseFromModel();
        var mensaje = $"Ha revocado una transferencia al cliente {transferenciaOrigenNew.NombreBeneficiario}, con un importe de {transferenciaOrigenNew.Importe}";
        await WebSocketHandler.SendToCliente(userAuth.Username, new Notificacion{Entity = userAuth.Username, Data = mensaje, Tipo = Tipo.DELETE, CreatedAt = DateTime.UtcNow.ToString()});

        return transferenciaResponse!;
    }
}