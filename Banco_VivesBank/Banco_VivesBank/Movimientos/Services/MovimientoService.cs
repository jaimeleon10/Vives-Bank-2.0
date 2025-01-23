using System.Numerics;
using Banco_VivesBank.Cliente.Exceptions;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Services;

public class MovimientoService : IMovimientoService
{
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private readonly ILogger<MovimientoService> _logger;
    private readonly IClienteService _clienteService;
    private readonly ICuentaService _cuentaService;
    private readonly ITarjetaService _tarjetaService;
    private readonly GeneralDbContext _context;

    
    public MovimientoService(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<MovimientoService> logger, IClienteService clienteService, ICuentaService cuentaService, GeneralDbContext context, ITarjetaService tarjetaService)
    {
        _logger = logger;
        _clienteService = clienteService;
        _cuentaService = cuentaService;
        _context = context;
        _tarjetaService = tarjetaService;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.MovimientosCollectionName);
        _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
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
        
        _logger.LogInformation($"Buscando movimiento con guid: {guid} en la base de datos");
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

    public async Task<IEnumerable<MovimientoResponse?>> GetByClienteGuidAsync(string clienteGuid)
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
        
        await _movimientoCollection.InsertOneAsync(nuevoMovimiento);
        _logger.LogInformation("Movimiento guardado con éxito");
    }

    public async Task<DomiciliacionResponse> CreateDomiciliacionAsync(DomiciliacionRequest domiciliacionRequest)
    {
        _logger.LogInformation("Creando domiciliación");
        
        _logger.LogInformation($"Buscando si existe el cliente con guid: {domiciliacionRequest.ClienteGuid}");
        var cliente = await _clienteService.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid);
        if (cliente == null)
        {
            _logger.LogWarning($"No se ha encontrado ningún cliente con guid: {domiciliacionRequest.ClienteGuid}");
            throw new ClienteNotFoundException($"No se ha encontrado ningún cliente con guid: {domiciliacionRequest.ClienteGuid}");
        }
        
        _logger.LogInformation($"Validando existencia de la cuenta con iban: {domiciliacionRequest.IbanCliente}");
        var cuenta = await _cuentaService.GetByIbanAsync(domiciliacionRequest.IbanCliente);
        if (cuenta == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
        }
        
        _logger.LogInformation($"Comprobando que el iban con guid: {domiciliacionRequest.IbanCliente} pertenezca a alguna de las cuentas del cliente con guid: {domiciliacionRequest.ClienteGuid}");
        if (!cliente.Cuentas.Any(c => c.Iban == domiciliacionRequest.IbanCliente)) // No aceptar sugerencia IDE
        {
            _logger.LogWarning($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {domiciliacionRequest.ClienteGuid}");
            throw new MovimientoException($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {domiciliacionRequest.ClienteGuid}");
        }
        
        _logger.LogInformation($"Validando saldo suficiente respecto al importe de: {domiciliacionRequest.Importe} €");
        if (!BigInteger.TryParse(domiciliacionRequest.Importe, out var importe))
        {
            throw new MovimientoException("El importe proporcionado no es un número entero válido.");
        }
        if (cuenta.Saldo < importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €");
            throw new MovimientoException($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();

            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo -= importe;

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
            throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuenta.Guid}\n\n{ex.Message}");
        }

        Domiciliacion domiciliacion = new Domiciliacion
        {
            ClienteGuid = cliente.Guid,
            Acreedor = domiciliacionRequest.Acreedor,
            IbanEmpresa = domiciliacionRequest.IbanEmpresa,
            IbanCliente = domiciliacionRequest.IbanCliente,
            Importe = importe,
            Periodicidad = (Periodicidad)Enum.Parse(typeof(Periodicidad), domiciliacionRequest.Periodicidad),
            Activa = domiciliacionRequest.Activa
        };
        
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);
        _logger.LogInformation("Domiciliación realizada con éxito");

        MovimientoRequest movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = cliente.Guid,
            Domiciliacion = domiciliacion,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = null
        };
        
        await CreateAsync(movimientoRequest);
        _logger.LogInformation("Movimiento del pago inicial de la domiciliación generado con éxito");

        return domiciliacion.ToResponseFromModel()!;
    }

    public async Task<IngresoNominaResponse> CreateIngresoNominaAsync(IngresoNominaRequest ingresoNominaRequest)
    {
        _logger.LogInformation("Creando ingreso de nómina");
        
        _logger.LogInformation($"Validando existencia de la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
        var cuenta = await _cuentaService.GetByIbanAsync(ingresoNominaRequest.IbanCliente);
        if (cuenta == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {ingresoNominaRequest.IbanCliente}");
        }
        
        if (!BigInteger.TryParse(ingresoNominaRequest.Importe, out var importe))
        {
            throw new MovimientoException("El importe proporcionado no es un número entero válido.");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();
            
            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo += importe;

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
            throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuenta.Guid}\n\n{ex.Message}");
        }
        
        IngresoNomina ingresoNomina = new IngresoNomina
        {
            NombreEmpresa = ingresoNominaRequest.NombreEmpresa,
            CifEmpresa = ingresoNominaRequest.CifEmpresa,
            IbanEmpresa = ingresoNominaRequest.IbanEmpresa,
            IbanCliente = ingresoNominaRequest.IbanCliente,
            Importe = importe
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
        _logger.LogInformation("Movimiento del pago inicial de la domiciliación generado con éxito");

        return ingresoNomina.ToResponseFromModel()!;
    }

    public async Task<PagoConTarjetaResponse> CreatePagoConTarjetaAsync(PagoConTarjetaRequest pagoConTarjetaRequest)
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
        
        _logger.LogInformation($"Validando saldo suficiente en la cuenta con guid: {cuenta.Guid} perteneciente a la tarjeta con guid: {tarjeta.Guid}");
        if (!BigInteger.TryParse(pagoConTarjetaRequest.Importe, out var importe))
        {
            throw new MovimientoException("El importe proporcionado no es un número entero válido.");
        }
        
        if (cuenta.Saldo < importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {pagoConTarjetaRequest.Importe} €");
            throw new MovimientoException($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {pagoConTarjetaRequest.Importe} €");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdate = await _context.Cuentas.Where(c => c.Guid == cuenta.Guid).FirstOrDefaultAsync();

            if (cuentaUpdate != null)
            {
                cuentaUpdate.Saldo -= importe;

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
            throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuenta.Guid}\n\n{ex.Message}");
        }
        
        PagoConTarjeta pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = pagoConTarjetaRequest.NombreComercio,
            Importe = importe,
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
        _logger.LogInformation("Movimiento del pago inicial de la domiciliación generado con éxito");

        return pagoConTarjeta.ToResponseFromModel()!;
    }

    public async Task<TransferenciaResponse> CreateTransferenciaAsync(TransferenciaRequest transferenciaRequest)
    {
        _logger.LogInformation("Creando transferencia");
        
        _logger.LogInformation("Validando existencia de la cuenta de destino");
        var cuentaDestino = await _cuentaService.GetByIbanAsync(transferenciaRequest.IbanDestino);
        if (cuentaDestino == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {transferenciaRequest.IbanDestino}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {transferenciaRequest.IbanDestino}");
        }
        
        _logger.LogInformation("Validando saldo suficiente en la cuenta de origen en caso de pertenecer a VivesBank");
        var cuentaOrigen = await _cuentaService.GetByIbanAsync(transferenciaRequest.IbanOrigen);
        
        if (!BigInteger.TryParse(transferenciaRequest.Importe, out var importe))
        {
            throw new MovimientoException("El importe proporcionado no es un número entero válido.");
        }
        
        if (cuentaOrigen != null)
        {
            if (cuentaOrigen.Saldo < importe)
            {
                _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuentaOrigen.Guid} respecto al importe de {transferenciaRequest.Importe} €");
                throw new MovimientoException($"Saldo insuficiente en la cuenta con guid: {cuentaOrigen.Guid} respecto al importe de {transferenciaRequest.Importe} ���");
            }
            
            await using var transactionCuentaOrigen = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaUpdateOrigen = await _context.Cuentas.Where(c => c.Guid == cuentaOrigen.Guid).FirstOrDefaultAsync();

                if (cuentaUpdateOrigen != null)
                {
                    cuentaUpdateOrigen.Saldo -= importe;

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
                throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaOrigen.Guid}\n\n{ex.Message}");
            }
        }
        
        await using var transactionCuentaDestino = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdateDestino = await _context.Cuentas.Where(c => c.Guid == cuentaDestino.Guid).FirstOrDefaultAsync();

            if (cuentaUpdateDestino != null)
            {
                cuentaUpdateDestino.Saldo += importe;

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
            throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaDestino.Guid}\n\n{ex.Message}");
        }
        
        Transferencia transferencia = new Transferencia
        {
            IbanOrigen = transferenciaRequest.IbanOrigen,
            NombreBeneficiario = transferenciaRequest.NombreBeneficiario,
            IbanDestino = transferenciaRequest.IbanDestino,
            Importe = importe
        };
        
        _logger.LogInformation("Transferencia realizada con éxito");

        if (cuentaOrigen != null)
        {
            MovimientoRequest movimientoRequestOrigen = new MovimientoRequest
            {
                ClienteGuid = cuentaOrigen.ClienteGuid,
                Domiciliacion = null,
                IngresoNomina = null,
                PagoConTarjeta = null,
                Transferencia = transferencia
            };
            
            await CreateAsync(movimientoRequestOrigen);
            _logger.LogInformation("Movimiento de la transferencia en la cuenta de origen generado con éxito");
        }

        MovimientoRequest movimientoRequestDestino = new MovimientoRequest
        {
            ClienteGuid = cuentaDestino.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = transferencia
        };
        
        await CreateAsync(movimientoRequestDestino);
        _logger.LogInformation("Movimiento de la transferencia en la cuenta de destino generado con éxito");

        return transferencia.ToResponseFromModel()!;
    }

    public async Task<TransferenciaResponse> RevocarTransferenciaAsync(string movimientoGuid)
    {
        _logger.LogInformation("Revocando transferencia");
        
        _logger.LogInformation($"Buscando movimiento de la transferencia con guid: {movimientoGuid}");
        var movimiento = await _movimientoCollection.Find(m => m.Guid == movimientoGuid).FirstOrDefaultAsync();
        if (movimiento == null || movimiento.Transferencia == null)
        {
            _logger.LogWarning($"No existe el movimiento con guid: {movimientoGuid} o no es un movimiento de transferencia");
            throw new MovimientoException($"No existe el movimiento con guid: {movimientoGuid} o no es un movimiento de transferencia");
        }
        
        _logger.LogInformation("Validando existencia de la cuenta de origen en caso de pertenecer a VivesBank");
        var cuentaOrigen = await _cuentaService.GetByIbanAsync(movimiento.Transferencia.IbanOrigen);
        if (cuentaOrigen != null)
        {
            await using var transactionCuentaOrigen = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaUpdateOrigen = await _context.Cuentas.Where(c => c.Guid == cuentaOrigen.Guid).FirstOrDefaultAsync();

                if (cuentaUpdateOrigen != null)
                {
                    cuentaUpdateOrigen.Saldo += movimiento.Transferencia.Importe;

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
                throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaOrigen.Guid}\n\n{ex.Message}");
            }
        }
        
        _logger.LogInformation("Validando existencia de la cuenta de destino");
        var cuentaDestino = await _cuentaService.GetByIbanAsync(movimiento.Transferencia.IbanDestino);
        if (cuentaDestino == null)
        {
            _logger.LogWarning($"No se ha encontrado la cuenta con iban: {movimiento.Transferencia.IbanDestino}");
            throw new CuentaNotFoundException($"No se ha encontrado la cuenta con iban: {movimiento.Transferencia.IbanDestino}");
        }

        if (cuentaDestino.Saldo < movimiento.Transferencia.Importe)
        {
            _logger.LogWarning($"Saldo insuficiente en la cuenta con guid: {cuentaDestino.Guid} respecto al importe de {movimiento.Transferencia.Importe} €");
            throw new MovimientoException($"Saldo insuficiente en la cuenta con guid: {cuentaDestino.Guid} respecto al importe de {movimiento.Transferencia.Importe} €");
        }
        
        await using var transactionCuentaDestino = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaUpdateDestino = await _context.Cuentas.Where(c => c.Guid == cuentaDestino.Guid).FirstOrDefaultAsync();

            if (cuentaUpdateDestino != null)
            {
                cuentaUpdateDestino.Saldo -= movimiento.Transferencia.Importe;

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
            throw new MovimientoException($"Error a la hora de actualizar el saldo de la cuenta con guid: {cuentaDestino.Guid}\n\n{ex.Message}");
        }
        
        Transferencia transferencia = new Transferencia
        {
            IbanOrigen = movimiento.Transferencia.IbanOrigen,
            NombreBeneficiario = movimiento.Transferencia.NombreBeneficiario,
            IbanDestino = movimiento.Transferencia.IbanDestino,
            Importe = movimiento.Transferencia.Importe
        };
        
        _logger.LogInformation("Transferencia revocada con éxito");

        if (cuentaOrigen != null)
        {
            MovimientoRequest movimientoRequestOrigen = new MovimientoRequest
            {
                ClienteGuid = cuentaOrigen.ClienteGuid,
                Domiciliacion = null,
                IngresoNomina = null,
                PagoConTarjeta = null,
                Transferencia = transferencia
            };
            
            await CreateAsync(movimientoRequestOrigen);
            _logger.LogInformation("Movimiento de la revocación de transferencia en la cuenta de origen generado con éxito");
        }

        MovimientoRequest movimientoRequestDestino = new MovimientoRequest
        {
            ClienteGuid = cuentaDestino.ClienteGuid,
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = transferencia
        };
        
        await CreateAsync(movimientoRequestDestino);
        _logger.LogInformation("Movimiento de la revocación de transferencia en la cuenta de destino generado con éxito");

        return transferencia.ToResponseFromModel()!;
    }
}