using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Banco_VivesBank.Movimientos.utils;

public class DomiciliacionScheduler : BackgroundService
{
    private readonly ILogger<DomiciliacionScheduler> _logger;
    private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private readonly GeneralDbContext _context;
    private readonly IMovimientoService _movimientoService;
    
    public DomiciliacionScheduler(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<DomiciliacionScheduler> logger, GeneralDbContext context, IMovimientoService movimientoService)
    {
        _logger = logger;
        _context = context;
        _movimientoService = movimientoService;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando el servicio de procesamiento de domiciliaciones");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarDomiciliacionesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar domiciliaciones");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcesarDomiciliacionesAsync()
    {
        _logger.LogInformation("Procesando domiciliaciones periódicas");

        var ahora = DateTime.UtcNow;
        var domiciliaciones = await _domiciliacionCollection.Find(dom => dom.Activa && RequiereEjecucion(dom, ahora)).ToListAsync();

        foreach (var domiciliacion in domiciliaciones)
        {
            try
            {
                _logger.LogInformation($"Ejecutando domiciliación: {domiciliacion.Guid}");
                await EjecutarDomiciliacionAsync(domiciliacion);

                domiciliacion.UltimaEjecucion = ahora;
                
                var filter = Builders<Domiciliacion>.Filter.Eq(d => d.Guid, domiciliacion.Guid);
                var update = Builders<Domiciliacion>.Update.Set(d => d.UltimaEjecucion, domiciliacion.UltimaEjecucion);
                await _domiciliacionCollection.UpdateOneAsync(filter, update);
            }
            catch (MovimientoException)
            {
                _logger.LogWarning($"Saldo insuficiente para domiciliación: {domiciliacion.Guid}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se ha podido procesar la domiciliación: {domiciliacion.Guid}\n\n{ex.Message}");
            }
        }
    }

    private bool RequiereEjecucion(Domiciliacion domiciliacion, DateTime ahora)
    {
        return domiciliacion.Periodicidad switch
        {
            Periodicidad.Diaria => domiciliacion.UltimaEjecucion.AddDays(1) <= ahora,
            Periodicidad.Semanal => domiciliacion.UltimaEjecucion.AddDays(7) <= ahora,
            Periodicidad.Mensual => domiciliacion.UltimaEjecucion.AddMonths(1) <= ahora,
            Periodicidad.Anual => domiciliacion.UltimaEjecucion.AddYears(1) <= ahora,
            _ => false
        };
    }

    private async Task EjecutarDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        var cuentaCliente = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == domiciliacion.IbanCliente);

        var saldoActual = cuentaCliente!.Saldo;
        var importe = domiciliacion.Importe;

        if (saldoActual < importe)
        {
            throw new MovimientoException($"La cuenta con iban {cuentaCliente.Iban} no tiene saldo suficiente para tramitar la domiciliación");
        }

        cuentaCliente.Saldo -= importe;
        _context.Cuentas.Update(cuentaCliente);

        var clienteDomiciliacion = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == cuentaCliente.ClienteId);

        var movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = clienteDomiciliacion.Guid,
            Domiciliacion = domiciliacion,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = null
        };

        await _movimientoService.CreateAsync(movimientoRequest);
    }
}