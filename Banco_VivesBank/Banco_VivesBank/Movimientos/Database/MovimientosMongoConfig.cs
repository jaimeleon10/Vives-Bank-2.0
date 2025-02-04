using MongoDB.Bson;
using MongoDB.Driver;

namespace Banco_VivesBank.Movimientos.Database;

public class MovimientosMongoConfig
{
    private readonly ILogger _logger;
    
    public MovimientosMongoConfig(ILogger<MovimientosMongoConfig> logger)
    {
        _logger = logger;
    }
    
    public MovimientosMongoConfig(){}
    
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string MovimientosCollectionName { get; set; } = string.Empty;
    public string DomiciliacionesCollectionName { get; set; } = string.Empty;
}