using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Services;

public class MovimientoService : IMovimientoService
{
    private readonly IMongoCollection<Movimiento> _movimientoCollection;
    private readonly ILogger<MovimientoService> _logger;
    
    public MovimientoService(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings, ILogger<MovimientoService> logger)
    {
        _logger = logger;
        var mongoClient = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>(movimientosDatabaseSettings.Value.CategoriasCollectionName);
    }
    
    public async Task<IEnumerable<Movimiento>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todos los movimientos en la base de datos");
        return await _movimientoCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Movimiento?> GetByIdAsync(string id)
    {
        _logger.LogInformation($"Buscando movimiento con id: {id}");
        
        if (!ObjectId.TryParse(id, out var objectId))
        {
            _logger.LogWarning($"Id con formáto inválido, debe ser un ObjectId: {id}");
            return null;
        }
        
        // Creamos la clave de cache
        // var cacheKey = CacheKeyPrefix + id;
        
        _logger.LogInformation("Buscando movimiento en la caché");
        // Intentamos obtener el movimiento de la caché
        // Lógica de obtención del movimiento en caché
        
        _logger.LogInformation("Buscando movimiento en base de datos");
        var movimiento = await _movimientoCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        
        if (movimiento != null)
        {
            _logger.LogInformation("Movimiento encontrado en base de datos.");

            // Si la encontramos, la cacheamos por 30 minutos
            //_logger.LogInformation("Cacheando la categoría.");
            //_memoryCache.Set(cacheKey, categoria, TimeSpan.FromMinutes(30));
            
            return movimiento;
        }
        else
        {
            _logger.LogInformation($"Movimiento no encontrado con id: {id}");
        }

        return null;
    }

    public async Task<Movimiento> CreateAsync(Movimiento movimiento)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> CreateDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> CreateIngresoNominaAsync(IngresoNomina ingresoNomina)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> CreatePagoConTarjetaAsync(PagoConTarjeta pagoConTarjeta)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> CreateTransferenciaAsync(Transferencia transferencia)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> RevocarTransferencia(string movimientoGuid)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento?> UpdateAsync(string id, Movimiento movimiento)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento?> DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }
}