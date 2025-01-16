using StackExchange.Redis;
using Vives_Bank_Net.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//redis 
string redisConnectionString = "localhost:6379,password=password123,abortConnect=false";

// Registrar los servicios
builder.Services.AddSingleton(typeof(IStorageJson), typeof(StorageJson));
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Añadimos Redis al contenedor de dependencias con manejo de excepciones
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    try
    {
        return ConnectionMultiplexer.Connect(redisConnectionString);
    }
    catch (Exception ex)
    {
        //logger.Error(ex, "🔴 Error connecting to Redis");
        throw new InvalidOperationException("Failed to connect to Redis", ex);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();