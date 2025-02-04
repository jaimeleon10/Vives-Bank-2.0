using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Config.Storage.Ftp;
using Banco_VivesBank.Database;
using Banco_VivesBank.Frankfurter.Services;
using Banco_VivesBank.GraphQL;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Scheduler;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Services;
using Banco_VivesBank.Producto.ProductoBase.Storage;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Storage.Ftp.Service;
using Banco_VivesBank.Storage.Pdf.Services;
using Banco_VivesBank.Storage.Images.Service;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Services;
using Banco_VivesBank.Swagger.Examples.Clientes;
using Banco_VivesBank.Swagger.Examples.Movimientos;
using Banco_VivesBank.Swagger.Examples.User;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Auth.Jwt;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Websockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Quartz;
using Serilog;
using Serilog.Core;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using Path = System.IO.Path;

var environment = InitLocalEnvironment();

// Init App Configuration
var configuration = InitConfiguration();

// Iniciamos la configuración externa de la aplicación
var logger = InitLogConfig();

// Iniciamos datos en MongoDB
RunInitMongoScriptIfEmpty();

// Inicializamos los servicios de la aplicación
var builder = InitServices();

var app = builder.Build();

// Si estamos en modo desarrollo, habilita Swagger/OpenAPI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prueba Swagger API v1");
    });
}

// Habilita redirección HTTPS si está habilitado
app.UseHttpsRedirection();

// Añadir esto si utilizas MVC para definir rutas, decimos que activamos el uso de rutas
app.UseRouting();

// Añadir para la autorización
app.UseAuthentication();
app.UseAuthorization();

// Añadir los websockets
app.UseWebSockets();

app.Map("/ws/api/bancovivesbank", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Only WebSocket requests are supported.");
        return;
    }

    // Verify authentication
    var authHeader = context.Request.Headers["Authorization"].ToString();

    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized: No token provided");
        return;
    }

    var token = authHeader["Bearer ".Length..].Trim();
    var username = ValidateToken(token);
    if (username == null)
    {
        Console.WriteLine("Invalid or expired token.");
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized: Invalid token");
        return;
    }
    Console.WriteLine($"Authenticated user: {username}");

    // If the token is valid, accept the WebSocket
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var handler = new WebSocketHandler(webSocket, username);
    await handler.Handle();
});

// Añade los controladores a la ruta predeterminada
app.MapControllers();

app.MapGraphQL(); // Accesible en /graphql


Console.WriteLine($"🕹️ Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");

logger.Information($"🕹️ Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");

// Inicia la aplicación web
app.Run();


WebApplicationBuilder InitServices()
{
    var myBuilder = WebApplication.CreateBuilder(args);

    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

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
    
    // Services
    myBuilder.Services.AddScoped<IUserService, UserService>();
    myBuilder.Services.AddScoped<IClienteService, ClienteService>();
    myBuilder.Services.AddScoped<IProductoService, ProductoService>();
    myBuilder.Services.AddScoped<ITarjetaService, TarjetaService>();
    myBuilder.Services.AddScoped<ICuentaService, CuentaService>();
    myBuilder.Services.AddScoped<IMovimientoService, MovimientoService>();
    myBuilder.Services.AddScoped<IDomiciliacionService, DomiciliacionService>();
    myBuilder.Services.AddScoped<IPdfStorage, PdfStorage>();
    myBuilder.Services.AddScoped<IFileStorageService, FileStorageService>();
    myBuilder.Services.AddScoped<IStorageProductos, StorageProductos>();
    myBuilder.Services.AddScoped<IBackupService, BackupService>();
    myBuilder.Services.AddScoped<IStorageJson, StorageJson>();
    myBuilder.Services.AddHttpClient();
    myBuilder.Services.AddScoped<IDivisasService, DivisasService>();
    myBuilder.Services.AddScoped<PaginationLinksUtils>();
    myBuilder.Services.AddScoped<DomiciliacionScheduler>();
    myBuilder.Services.AddScoped<DomiciliacionJob>();
    myBuilder.Services.AddHttpContextAccessor();
    
    // Quartz (domiciliaciones)
    myBuilder.Services.AddQuartz(q =>
    {
        q.UseSimpleTypeLoader();

        // Configure Job and Trigger
        var jobKey = new JobKey("DomiciliacionJob");
        q.AddJob<DomiciliacionJob>(opts => opts.WithIdentity(jobKey));
        
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("DomiciliacionJob-Trigger")
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(86400) // Cada 24 horas se revisan las domiciliaciones
                .RepeatForever()));
    });
    
    // Quartz Hosted Service
    myBuilder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    
    //Ftp
    myBuilder.Services.Configure<FtpConfig>(myBuilder.Configuration.GetSection("FtpSettings"));
    myBuilder.Services.AddScoped<IFtpService, FtpService>();
    
    // Caché en memoria
    myBuilder.Services.AddMemoryCache();

    // Base de datos en PostgreSQL
    myBuilder.Services.AddDbContext<GeneralDbContext>(options =>
        options.UseNpgsql(myBuilder.Configuration.GetConnectionString("DefaultConnection")));

    // Añadimos los controladores
    myBuilder.Services.AddControllers();
    
    // Registra el servidor GraphQL y asigna el Query type.
    myBuilder.Services.AddGraphQLServer()
        .AddQueryType<MovimientoQuery>();

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
                Name = "Jaime León Mulero",
                Email = "jleonmulero@gmail.com",
                Url = new Uri("https://github.com/jaimeleon10")
            }
        });
        c.ExampleFilters();  //Habilita los ejemplos de las clases
        var xmlFile = Path.Combine(AppContext.BaseDirectory, "Banco_VivesBank.xml");
           
        c.IncludeXmlComments(xmlFile);
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
    
    //Auth
    myBuilder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = myBuilder.Configuration["Jwt:Issuer"],
            ValidAudience = myBuilder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(myBuilder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Acceso no autorizado. Debe iniciar sesión para acceder al recurso solicitado.",
                    path = context.Request.Path
                }));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Acceso denegado. No tiene los permisos requeridos para acceder al recurso solicitado.",
                    path = context.Request.Path
                }));
            }
        };
    });
    
    // Importante aquí definimos los roles de los usuarios permitidos
    myBuilder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
        options.AddPolicy("ClientePolicy", policy => policy.RequireRole("Cliente"));
        options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
        options.AddPolicy("ClienteOrUserPolicy", policy => policy.RequireRole("Cliente", "User"));
    });
    
    myBuilder.Services.AddScoped<IJwtService, JwtService>();
    
    // Añadir los ejemplos de las clases
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<ClienteResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<PageResponseClienteExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<UserResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<DomiciliacionResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<IngresoNominaResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<MovimientoResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<PagoConTarjetaResponseExample>();
    myBuilder.Services.AddSwaggerExamplesFromAssemblyOf<TransferenciaResponseExample>();
    
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

void RunInitMongoScriptIfEmpty()
{
    var connectionString = configuration.GetSection("MovimientosDatabase:ConnectionString").Value;
    var databaseName = configuration.GetSection("MovimientosDatabase:DatabaseName").Value;

    // Leer los nombres de las colecciones desde la configuración
    var movimientosCollectionName = configuration.GetSection("MovimientosDatabase:MovimientosCollectionName").Value;
    var domiciliacionesCollectionName = configuration.GetSection("MovimientosDatabase:DomiciliacionesCollectionName").Value;

    // Validar que los valores no estén vacíos
    if (string.IsNullOrEmpty(movimientosCollectionName) || string.IsNullOrEmpty(domiciliacionesCollectionName))
    {
        throw new ArgumentNullException("Los nombres de las colecciones no están configurados correctamente en appsettings.json");
    }

    // Crear cliente de MongoDB y base de datos
    var client = new MongoClient(connectionString);
    var database = client.GetDatabase(databaseName);

    // Obtener las colecciones
    var movimientosCollection = database.GetCollection<BsonDocument>(movimientosCollectionName);
    var domiciliacionesCollection = database.GetCollection<BsonDocument>(domiciliacionesCollectionName);

    // Comprobar si las colecciones están vacías
    var movimientosVacios = movimientosCollection.CountDocuments(FilterDefinition<BsonDocument>.Empty) == 0;
    var domiciliacionesVacios = domiciliacionesCollection.CountDocuments(FilterDefinition<BsonDocument>.Empty) == 0;

    if (movimientosVacios || domiciliacionesVacios)
    {
        Console.WriteLine("🔄 Una o más colecciones están vacías. Ejecutando el script initMongoData.js...");
        RunInitMongoScript();
    }
    else
    {
        Console.WriteLine("✅ Todas las colecciones ya contienen datos. No es necesario ejecutar initMongoData.js.");
    }
}

void RunInitMongoScript()
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = "mongo/initMongoData.js",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();

    while (!process.StandardOutput.EndOfStream)
    {
        Console.WriteLine(process.StandardOutput.ReadLine());
    }

    while (!process.StandardError.EndOfStream)
    {
        Console.Error.WriteLine(process.StandardError.ReadLine());
    }

    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        throw new Exception("🔴 Error al ejecutar el script initMongoData.js");
    }
}

string? ValidateToken(string token)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("ClaveSecretaSuperSegura123JamasLaDescubriraNadieEnElPlanetaTierra!?159");

    try
    {
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        }, out SecurityToken validatedToken);

        return principal.Identity?.Name;
    }
    catch
    {
        return null;
    }
}