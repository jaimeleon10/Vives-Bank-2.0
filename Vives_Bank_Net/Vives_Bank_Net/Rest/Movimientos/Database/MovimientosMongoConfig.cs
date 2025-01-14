using MongoDB.Bson;
using MongoDB.Driver;

namespace Vives_Bank_Net.Rest.Movimientos.Database;

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
    public string CategoriasCollectionName { get; set; } = string.Empty;
    
    public void TryConnection()
    {
        _logger.LogInformation("Trying to connect to MongoDB");
        var settings = MongoClientSettings.FromConnectionString(ConnectionString);
        // Set the ServerApi field of the settings object to set the version of the Stable API on the client
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        // Create a new client and connect to the server
        var client = new MongoClient(settings);
        // Send a ping to confirm a successful connection
        try
        {
            client.GetDatabase("DatabaseName").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            _logger.LogInformation("🟢 You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔴 Error connecting to MongoDB");
            Environment.Exit(1);
        }
    }
}