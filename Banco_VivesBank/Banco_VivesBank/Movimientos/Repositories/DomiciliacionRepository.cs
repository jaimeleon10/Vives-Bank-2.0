using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Repositories;

public class DomiciliacionRepository : IDomiciliacionRepository
{
    private readonly IMongoCollection<Domiciliacion> _collection;
    private readonly ILogger<DomiciliacionRepository> _logger;

    public DomiciliacionRepository(IOptions<MovimientosMongoConfig> movimientosDatabaseSettings,
        ILogger<DomiciliacionRepository> logger)
    {
        var client = new MongoClient(movimientosDatabaseSettings.Value.ConnectionString);
        var database = client.GetDatabase(movimientosDatabaseSettings.Value.DatabaseName);
        _collection = database.GetCollection<Domiciliacion>(movimientosDatabaseSettings.Value.DomiciliacionesCollectionName);
        _logger = logger;
    }


    public async Task<List<Domiciliacion>> GetAllDomiciliacionesAsync()
    {
        _logger.LogInformation("Getting all direct debits from the database.");
        return await _collection.FindAsync(_ => true).Result.ToListAsync();
    }

    public async Task<List<Domiciliacion>> GetAllDomiciliacionesActivasAsync()
    {
        _logger.LogInformation("Getting all active direct debits from the database.");
        return await _collection.FindAsync(d => d.Activa).Result.ToListAsync();
    }

    public async Task<Domiciliacion> GetDomiciliacionByIdAsync(string id)
    {
        _logger.LogInformation($"Getting direct debit with id {id} from the database.");
        return await _collection.FindAsync(d => d.Id == id).Result.FirstOrDefaultAsync();
    }

    public async Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Adding a new direct debit to the database: {domiciliacion}");
        await _collection.InsertOneAsync(domiciliacion);
        return domiciliacion;
    }

    public async Task<Domiciliacion> UpdateDomiciliacionAsync(string id, Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Updating direct debit with id {id} in the database.");
        var updateResult = await _collection.FindOneAndReplaceAsync(
            d => d.Id == id,
            domiciliacion,
            new FindOneAndReplaceOptions<Domiciliacion>{ ReturnDocument = ReturnDocument.After }
        );
        return updateResult;
    }

    public async Task<Domiciliacion> DeleteDomiciliacionAsync(string id)
    {
        _logger.LogInformation($"Deleting direct debit with id {id} from the database.");
        var deletedDomiciliacion = await _collection.FindOneAndDeleteAsync(d => d.Id == id);
        return deletedDomiciliacion;    }

    public async Task<List<Domiciliacion>> GetDomiciliacionesActivasByClienteGiudAsync(string clienteGuid)
    {            
        _logger.LogInformation($"Getting active direct debits for client with guid {clienteGuid} from the database.");
        return await _collection.FindAsync(d => d.ClienteGuid == clienteGuid && d.Activa).Result.ToListAsync();
    }

    public async Task<List<Domiciliacion>> GetDomiciliacionByClientGuidAsync(string clientGuid)
    {
        _logger.LogInformation($"Getting direct debits for client with guid {clientGuid}.");
        return await _collection.FindAsync(d => d.ClienteGuid == clientGuid).Result.ToListAsync();
    }
}