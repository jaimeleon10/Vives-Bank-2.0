using System.Numerics;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Validators;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Services;

public class MovimientoService : IMovimientoService
{
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly ILogger<MovimientoService> _logger;
    private readonly IClienteService _clienteService;
    private readonly ICuentaService _cuentaService;
    
    public MovimientoService(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<MovimientoService> logger, IClienteService clienteService, ICuentaService cuentaService)
    {
        _logger = logger;
        _clienteService = clienteService;
        _cuentaService = cuentaService;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.CategoriasCollectionName);
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
        
        _logger.LogInformation("Buscando movimiento en base de datos");
        var movimiento = await _movimientoCollection.Find(m => m.Guid == guid).FirstOrDefaultAsync();
        
        if (movimiento != null)
        {
            _logger.LogInformation("Movimiento encontrado en base de datos.");
            return movimiento.ToResponseFromModel(
                movimiento.Domiciliacion.ToResponseFromModel(),
                movimiento.IngresoNomina.ToResponseFromModel(),
                movimiento.PagoConTarjeta.ToResponseFromModel(),
                movimiento.Transferencia.ToResponseFromModel());
        }
        
        _logger.LogInformation($"Movimiento no encontrado con guid: {guid}");
        return null;
    }

    public async Task<IEnumerable<MovimientoResponse?>> GetByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Buscando todos los movimientos del cliente con guid: {clienteGuid}");
        var movimientos = await _movimientoCollection.Find(mov => mov.Cliente.Guid == clienteGuid).ToListAsync();
        return movimientos.Select(mov => mov.ToResponseFromModel(
            mov.Domiciliacion.ToResponseFromModel(),
            mov.IngresoNomina.ToResponseFromModel(),
            mov.PagoConTarjeta.ToResponseFromModel(),
            mov.Transferencia.ToResponseFromModel()
        ));
    }

    public async Task<MovimientoResponse> CreateAsync(MovimientoRequest movimientoRequest)
    {
        _logger.LogInformation("Guardando movimiento");
        var clienteExistente = await _clienteService.GetClienteModelByGuid(movimientoRequest.ClienteGuid);
        if (clienteExistente == null)
        {
            _logger.LogInformation("El cliente asignado al movimiento no existe o no se ha encontrado");
            throw new ClienteNotFoundException("El cliente asignado al movimiento no existe o no se ha encontrado");
        }

        Movimiento nuevoMovimiento = new Movimiento
        {
            Cliente = clienteExistente,
            Domiciliacion = movimientoRequest.Domiciliacion,
            IngresoNomina = movimientoRequest.IngresoNomina,
            PagoConTarjeta = movimientoRequest.PagoConTarjeta,
            Transferencia = movimientoRequest.Transferencia
        };
        
        await _movimientoCollection.InsertOneAsync(nuevoMovimiento);
        _logger.LogInformation("Movimiento guardado con éxito");

        return nuevoMovimiento.ToResponseFromModel(
            nuevoMovimiento.Domiciliacion.ToResponseFromModel(),
            nuevoMovimiento.IngresoNomina.ToResponseFromModel(),
            nuevoMovimiento.PagoConTarjeta.ToResponseFromModel(),
            nuevoMovimiento.Transferencia.ToResponseFromModel()
        );
    }

    public async Task<MovimientoResponse> CreateDomiciliacionAsync(DomiciliacionRequest domiciliacionRequest)
    {
        _logger.LogInformation("Creando domiciliación");
        
        _logger.LogInformation("Buscando si existe el cliente proporcionado");
        var cliente = await _clienteService.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid);
        if (cliente == null)
        {
            _logger.LogInformation("El cliente proporcionado no existe");
            throw new ClienteNotFoundException("El cliente proporcionado no existe");
        }
        
        _logger.LogInformation("Validando cuenta del cliente existente");
        if (await _cuentaService.GetByIbanAsync(domiciliacionRequest.IbanCliente) == null)
        {
            _logger.LogInformation($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
            throw new CuentaNoEncontradaException($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}");
        }
        
        _logger.LogInformation("Comprobando que el iban del cliente coincida con alguna de las cuentas del cliente");
        if (!cliente.Cuentas.Any(cuenta => cuenta.Iban == domiciliacionRequest.IbanCliente)) // No aceptar sugerencia IDE
        {
            _logger.LogInformation("El iban del cliente no pertenece a ninguna cuenta del cliente proporcionado");
            throw new IbanNotExistsException("El iban del cliente no pertenece a ninguna cuenta del cliente proporcionado");
        }

        domiciliacionRequest.UltimaEjecuccion = DateTime.UtcNow;
        
        Domiciliacion domiciliacion = new Domiciliacion
        {
            Cliente = cliente,
            IbanEmpresa = domiciliacionRequest.IbanEmpresa,
            IbanCliente = domiciliacionRequest.IbanCliente,
            Importe = domiciliacionRequest.Importe,
            Acreedor = domiciliacionRequest.Acreedor,
            Periodicidad = domiciliacionRequest.Periodicidad,
            Comentarios = domiciliacionRequest.Comentarios
        }
        
        //await _movimientoCollection.InsertOneAsync(domiciliacion);
        _logger.LogInformation("Domiciliación generada con éxito");
        
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse> CreateIngresoNominaAsync(IngresoNominaRequest ingresoNomina)
    {
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse> CreatePagoConTarjetaAsync(PagoConTarjetaRequest pagoConTarjeta)
    {
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse> CreateTransferenciaAsync(TransferenciaRequest transferencia)
    {
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse> RevocarTransferencia(string movimientoGuid)
    {
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse?> UpdateAsync(string id, MovimientoRequest movimientoRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<MovimientoResponse?> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }
}