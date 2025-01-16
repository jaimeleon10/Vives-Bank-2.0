using System.Text;
using GraphiQl;
using GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
using Vives_Bank_Net.GraphQL;
using Vives_Bank_Net.Rest.Movimientos.Database;
using Vives_Bank_Net.Rest.Movimientos.Services;
using StackExchange.Redis;
using Vives_Bank_Net.Storage;

var environment = InitLocalEnvironment();

// Init App Configuration
var configuration = InitConfiguration();

// Iniciamos la configuración externa de la aplicación
var logger = InitLogConfig();

// Inicializamos los servicios de la aplicación
var builder = InitServices();

var app = builder.Build();

// Si estamos en modo desarrollo, habilita Swagger/OpenAPI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilita redirección HTTPS si está habilitado
app.UseHttpsRedirection();

// Añadir para la autorización
app.UseAuthorization();

// Añadir esto si utilizas MVC para definir rutas, decimos que activamos el uso de rutas
app.UseRouting();

// Añade los controladores a la ruta predeterminada
app.MapControllers();

// Middleware para GraphQL
app.MapGraphQL<MovimientoSchema>("/graphql");

// GraphQL Playground
app.UseGraphiQl("/graphiql");

Console.WriteLine($"🕹️ Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");

logger.Information($"🕹️ Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");

// Inicia la aplicación web
app.Run();


WebApplicationBuilder InitServices()
{
    var myBuilder = WebApplication.CreateBuilder(args);

    // Configuramos los servicios de la aplicación

    // Poner Serilog como logger por defecto (otra alternativa)
    myBuilder.Services.AddLogging(logging =>
    {
        logging.ClearProviders(); // Limpia los proveedores de log por defecto
        logging.AddSerilog(logger, true); // Añade Serilog como un proveedor de log
    });
    logger.Debug("Logger por defecto: Serilog");
    
    // Configuración de MongoDB
    myBuilder.Services.Configure<MovimientosMongoConfig>(
        myBuilder.Configuration.GetSection("MovimientosDatabase"));
    
    TryConnectionDataBase();
    
    myBuilder.Services.AddSingleton<IMovimientoService, MovimientoService>();
    myBuilder.Services.AddSingleton<IStorageJson, StorageJson>();
    myBuilder.Services.AddSingleton<IFileStorageService, FileStorageService>();

    // GraphQL
    myBuilder.Services.AddSingleton<MovimientoQuery>();
    myBuilder.Services.AddSingleton<MovimientoSchema>();
    myBuilder.Services.AddSingleton<MovimientoType>();
    
    myBuilder.Services.AddGraphQL(graphQlBuilder =>
    {
        graphQlBuilder.AddSystemTextJson();
    });

    // Añadimos los controladores
    myBuilder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle para documentar la API
    myBuilder.Services.AddEndpointsApiExplorer();
    myBuilder.Services.AddSwaggerGen(c =>
    {
        c.EnableAnnotations();
        // Otros metadatos de la API
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Vives-Bank",
            Description = "API para la gestión del banco Vives-Bank",
            Contact = new OpenApiContact
            {
                Name = "Jaime León Mulero, Natalia González Álvarez",
                Email = "jleonmulero@gmail.com, nagonal2004@gmail.com",
                Url = new Uri("https://github.com/jaimeleon10, https://github.com/ngalvez0910")
            }
        });
    });

    // redis 
    string redisConnectionString = "localhost:6379,password=password123,abortConnect=false";

    // Añadimos Redis al contenedor de dependencias con manejo de excepciones
    myBuilder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        try
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "🔴 Error connecting to Redis");
            throw new InvalidOperationException("Failed to connect to Redis", ex);
        }
    });

    return myBuilder;
}

string InitLocalEnvironment()
{
    Console.OutputEncoding = Encoding.UTF8; // Necesario para mostrar emojis
    Console.WriteLine("🤖 Proyecto Vives-Bank en .NET 🤖\n");
    var myEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
    return myEnvironment;
}

IConfiguration InitConfiguration()
{
    var myConfiguration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .Build();
    return myConfiguration;
}

Logger InitLogConfig()
{
    // Creamos un logger con la configuración de Serilog
    return new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

void TryConnectionDataBase()
{
    logger.Debug("Trying to connect to MongoDB");
    // Leemos la cadena de conexión a la base de datos desde la configuración
    var connectionString = configuration.GetSection("MovimientosDatabase:ConnectionString").Value;
    var settings = MongoClientSettings.FromConnectionString(connectionString);
    // Set the ServerApi field of the settings object to set the version of the Stable API on the client
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    // Create a new client and connect to the server
    var client = new MongoClient(settings);
    // Send a ping to confirm a successful connection
    try
    {
        client.GetDatabase("DatabaseName").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        logger.Information("🟢 You successfully connected to MongoDB!");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "🔴 Error connecting to , closing application");
        Environment.Exit(1);
    }
}