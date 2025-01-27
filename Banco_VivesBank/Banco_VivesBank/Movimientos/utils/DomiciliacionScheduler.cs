using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.utils;

public class DomiciliacionScheduler : BackgroundService
{
    private readonly ILogger<DomiciliacionScheduler> _logger;
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private readonly ICuentaService _cuentaService;

    public DomiciliacionScheduler(
        IOptions<MovimientosMongoConfig> movimientosDatabaseSettings,
        ILogger<DomiciliacionScheduler> logger,
        ICuentaService cuentaService)
    {
        _logger = logger;
        _cuentaService = cuentaService;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.MovimientosCollectionName);
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

        var ahora = DateTime.Now;
        var domiciliaciones = await _domiciliacionCollection.Find(dom => dom.Activa == true).ToListAsync()
            .Where(d => d.Activa && RequiereEjecucion(d, ahora))
            .ToList();

        foreach (var domiciliacion in domiciliaciones)
        {
            try
            {
                _logger.LogInformation($"Ejecutando domiciliación: {domiciliacion.Guid}");
                await EjecutarDomiciliacionAsync(domiciliacion);

                domiciliacion.UltimaEjecucion = ahora;
                await _domiciliacionCollection.UpdateAsync(domiciliacion);
            }
            catch (SaldoInsuficienteException ex)
            {
                _logger.LogWarning($"Saldo insuficiente para domiciliación: {domiciliacion.Guid}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar domiciliación: {domiciliacion.Guid}");
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
        var cuentaOrigen = await _cuentaService.GetByIbanAsync(domiciliacion.IbanOrigen);

        var saldoActual = cuentaOrigen.Saldo;
        var cantidad = domiciliacion.Cantidad;

        if (saldoActual < cantidad)
        {
            throw new SaldoInsuficienteException(cuentaOrigen.Iban, saldoActual);
        }

        // Actualizar saldos
        cuentaOrigen.Saldo -= cantidad;
        await _cuentaService.UpdateAsync(cuentaOrigen);

        // Registrar movimiento
        var movimiento = new Movimiento
        {
            ClienteGuid = cuentaOrigen.ClienteId,
            Domiciliacion = domiciliacion
        };

        await _movimientoCollection.SaveAsync(movimiento);
    }
}