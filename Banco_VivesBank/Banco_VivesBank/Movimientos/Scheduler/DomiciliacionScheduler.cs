using System.Transactions;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Scheduler
{
    public class DomiciliacionScheduler
    {
        private readonly ILogger<DomiciliacionScheduler> _logger;
        private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
        private readonly GeneralDbContext _context;
        private readonly IMovimientoService _movimientoService;

        public DomiciliacionScheduler(
            IOptions<MovimientosMongoConfig> movimientosDatabaseSettings,
            ILogger<DomiciliacionScheduler> logger,
            GeneralDbContext context,
            IMovimientoService movimientoService)
        {
            _logger = logger;
            var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
            _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
            _context = context;
            _movimientoService = movimientoService;
        }

        public async Task ProcesarDomiciliacionesAsync()
        {
            _logger.LogInformation("Procesando domiciliaciones periódicas");

            var ahora = DateTime.UtcNow;

            // Se filtran las domiciliaciones activas directamente con el Find
            var domiciliaciones = await _domiciliacionCollection
                .Find(dom => dom.Activa)
                .ToListAsync();

            // Filtrado de domiciliaciones que requieren ejecución
            var domiciliacionesRequierenEjecucion = new List<Domiciliacion>();
            foreach (var domiciliacion in domiciliaciones)
            {
                if (RequiereEjecucion(domiciliacion, ahora))
                {
                    domiciliacionesRequierenEjecucion.Add(domiciliacion);
                }
            }

            foreach (var domiciliacion in domiciliacionesRequierenEjecucion)
            {
                try
                {
                    _logger.LogInformation($"Ejecutando domiciliación: {domiciliacion.Guid}");
                    await EjecutarDomiciliacionAsync(domiciliacion);

                    domiciliacion.UltimaEjecucion = ahora;
                    
                    // Actualizar la última ejecución en MongoDB
                    var filter = Builders<Domiciliacion>.Filter.Eq(d => d.Guid, domiciliacion.Guid);
                    var update = Builders<Domiciliacion>.Update.Set(d => d.UltimaEjecucion, domiciliacion.UltimaEjecucion);
                    await _domiciliacionCollection.UpdateOneAsync(filter, update);
                }
                catch (MovimientoException)
                {
                    _logger.LogWarning($"Saldo insuficiente para domiciliación: {domiciliacion.Guid}");
                    domiciliacion.Activa = false;
                    
                    // Actualizar la última ejecución en MongoDB
                    var filter = Builders<Domiciliacion>.Filter.Eq(d => d.Guid, domiciliacion.Guid);
                    var update = Builders<Domiciliacion>.Update.Set(d => d.Activa, domiciliacion.Activa);
                    await _domiciliacionCollection.UpdateOneAsync(filter, update);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"No se ha podido procesar la domiciliación: {domiciliacion.Guid}\n\n{ex.Message}");
                }
            }
        }

        private bool RequiereEjecucion(Domiciliacion domiciliacion, DateTime ahora)
        {
            // Se evalúa la necesidad de ejecución según la periodicidad
            return domiciliacion.Periodicidad switch
            {
                Periodicidad.Diaria => domiciliacion.UltimaEjecucion.AddDays(1) < ahora,
                Periodicidad.Semanal => domiciliacion.UltimaEjecucion.AddDays(7) < ahora,
                Periodicidad.Mensual => domiciliacion.UltimaEjecucion.AddMonths(1) < ahora,
                Periodicidad.Anual => domiciliacion.UltimaEjecucion.AddYears(1) < ahora,
                _ => false
            };
        }

        private async Task EjecutarDomiciliacionAsync(Domiciliacion domiciliacion)
        {
            // Recuperar la cuenta cliente desde la base de datos
            var cuentaCliente = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == domiciliacion.IbanCliente);

            // Verificar si hay saldo suficiente en la cuenta
            var saldoActual = cuentaCliente!.Saldo;
            var importe = domiciliacion.Importe;

            if (saldoActual < importe)
            {
                throw new MovimientoException($"La cuenta con iban {cuentaCliente.Iban} no tiene saldo suficiente para tramitar la domiciliación");
            }

            // Actualizar el saldo de la cuenta
            await using var transactionCuenta = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuentaCliente.Guid).FirstOrDefaultAsync();

                if (cuentaUpdate != null)
                {
                    cuentaUpdate.Saldo -= domiciliacion.Importe;

                    await _context.SaveChangesAsync();
                    await transactionCuenta.CommitAsync();
                }
                else
                {
                    await transactionCuenta.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transactionCuenta.RollbackAsync();
                _logger.LogWarning($"Error al actualizar el saldo de la cuenta con guid: {cuentaCliente.Guid}");
                throw new TransactionException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaCliente.Guid}\n\n{ex.Message}");
            }

            // Obtener el cliente de la domiciliación
            var clienteDomiciliacion = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == cuentaCliente.ClienteId);

            // Crear y procesar el movimiento de la domiciliación
            var movimientoRequest = new MovimientoRequest
            {
                ClienteGuid = clienteDomiciliacion!.Guid,
                Domiciliacion = domiciliacion,
                IngresoNomina = null,
                PagoConTarjeta = null,
                Transferencia = null
            };

            await _movimientoService.CreateAsync(movimientoRequest);
        }
    }
}
