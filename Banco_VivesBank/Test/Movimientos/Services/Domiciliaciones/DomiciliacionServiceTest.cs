
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Cuenta.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Testcontainers.MongoDb;

[TestFixture]
[TestOf(typeof(DomiciliacionService))]
public class DomiciliacionServiceTest
{
    private MongoDbContainer _mongoDbContainer;
    private IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private DomiciliacionService _domiciliacionService;
    private GeneralDbContext _dbContext;
    private Mock<IClienteService> _clienteService;
    private Mock<ICuentaService> _cuentaService;
    private Mock<IMovimientoService> _movimientoService;
    private Mock<IOptions<MovimientosMongoConfig>> _mongoConfigMock;
    private Mock<ILogger<DomiciliacionService>> _loggerMock;
    private IMemoryCache _memoryCache;
    private Mock<IConnectionMultiplexer> _redis;
    private Mock<IDatabase> _redisDatabase;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:5.0")
            .WithExposedPort(27017)
            .Build();
        
        await _mongoDbContainer.StartAsync();
        
        var mongoClient = new MongoClient(_mongoDbContainer.GetConnectionString());
        var mongoDatabase = mongoClient.GetDatabase("testdb");
        _domiciliacionCollection = mongoDatabase.GetCollection<Domiciliacion>("domiciliaciones");
        
        _clienteService = new Mock<IClienteService>();
        _cuentaService = new Mock<ICuentaService>();
        _movimientoService = new Mock<IMovimientoService>();
        _mongoConfigMock = new Mock<IOptions<MovimientosMongoConfig>>();
        _loggerMock = new Mock<ILogger<DomiciliacionService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _redis = new Mock<IConnectionMultiplexer>();
        _redisDatabase = new Mock<IDatabase>();
        
        _mongoConfigMock.Setup(x => x.Value).Returns(new MovimientosMongoConfig
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb",
            DomiciliacionesCollectionName = "domiciliaciones"
        });
        
        _redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDatabase.Object);
        
        _domiciliacionService = new DomiciliacionService(
            _mongoConfigMock.Object,    
            _loggerMock.Object,         
            _clienteService.Object,  
            _cuentaService.Object,  
            _dbContext, 
            _redis.Object,
            _memoryCache,
            _movimientoService.Object
        );
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_mongoDbContainer != null)
        {
            await _mongoDbContainer.StopAsync();
            await _mongoDbContainer.DisposeAsync();
        }
        
        if (_memoryCache != null)
        {
            _memoryCache.Dispose();
        }
    }
   

    [Test]
    public async Task GetAllAsync()
    {
       
        var domiciliacion1 = new Domiciliacion
        {
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };

        var domiciliacion2 = new Domiciliacion
        {
            ClienteGuid = "cliente-2",
            Acreedor = "Acreedor2",
            IbanEmpresa = "ES2345678901234567890123",
            IbanCliente = "ES8765432109876543210987",
            Importe = 300,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };

        await _domiciliacionCollection.InsertManyAsync(new List<Domiciliacion> { domiciliacion1, domiciliacion2 });
        
        var result = await _domiciliacionService.GetAllAsync();
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Acreedor, Is.EqualTo("Acreedor1"));
        Assert.That(result.Last().Acreedor, Is.EqualTo("Acreedor2"));
    }
    
    [Test]
    public async Task GetAllAsync_SinDomiciliaciones()
    {
        var result = await _domiciliacionService.GetAllAsync();
        Assert.That(result.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task GetByGuidAsync_MemoryCache()
    {
        
        var domiciliacionGuid = "guid-123";
        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };

        _memoryCache.Set(domiciliacionGuid, domiciliacion);
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Acreedor, Is.EqualTo("Acreedor1"));
    }
    
    /*[Test]
    public async Task GetByGuidAsync_Redis()
    {
        
        var domiciliacionGuid = "guid-123";
        
        var domiciliacionResponse = new DomiciliacionResponse
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual", 
            Activa = true,
            FechaInicio = DateTime.UtcNow.ToString("yyyy-MM-dd"), 
            UltimaEjecuccion = DateTime.UtcNow.ToString("yyyy-MM-dd") 
        };

      
        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual, 
            Activa = true,
            FechaInicio = DateTime.UtcNow, 
            UltimaEjecucion = DateTime.UtcNow 
        };
        
        var redisValue = JsonSerializer.Serialize(domiciliacion);
        
        var cacheKey = $"Domiciliacion:{domiciliacionGuid}";
        _redisDatabase.Setup(db => db.StringGetAsync(It.Is<RedisKey>(key => key == cacheKey), It.IsAny<CommandFlags>()))
                      .ReturnsAsync(redisValue);
        
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Acreedor, Is.EqualTo(domiciliacionResponse.Acreedor));
        Assert.That(result.Guid, Is.EqualTo(domiciliacionResponse.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(domiciliacionResponse.ClienteGuid));
        Assert.That(result.Importe, Is.EqualTo(domiciliacionResponse.Importe));
        Assert.That(result.Periodicidad, Is.EqualTo(domiciliacionResponse.Periodicidad));
        Assert.That(result.Activa, Is.EqualTo(domiciliacionResponse.Activa));
        Assert.That(result.FechaInicio, Is.EqualTo(domiciliacionResponse.FechaInicio));
        Assert.That(result.UltimaEjecuccion, Is.EqualTo(domiciliacionResponse.UltimaEjecuccion));
        
    }

    [Test]
    public async Task CreateAsync_ShouldCreateDomiciliacionSuccessfully()
    {
        // Arrange
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };
        
        var cuenta = new Cuenta
        {
            Guid = "cuenta-123",
            ClienteGuid = "cliente-123",
            Iban = "ES9876543210987654321098",
            Saldo = 1000 
        };

        var expectedDomiciliacion = new Domiciliacion
        {
            ClienteGuid = domiciliacionRequest.ClienteGuid,
            Acreedor = domiciliacionRequest.Acreedor,
            IbanEmpresa = domiciliacionRequest.IbanEmpresa,
            IbanCliente = domiciliacionRequest.IbanCliente,
            Importe = domiciliacionRequest.Importe,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta);

        _domiciliacionCollection.Setup(d => d.InsertOneAsync(It.IsAny<Domiciliacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _movimientoService.Setup(m => m.CreateAsync(It.IsAny<MovimientoRequest>()))
            .Returns(Task.CompletedTask);
        
        var result = await _domiciliacionService.CreateAsync(domiciliacionRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Acreedor, Is.EqualTo(domiciliacionRequest.Acreedor));
        Assert.That(result.IbanCliente, Is.EqualTo(domiciliacionRequest.IbanCliente));
        Assert.That(result.Importe, Is.EqualTo(domiciliacionRequest.Importe));
        Assert.That(result.Periodicidad, Is.EqualTo(domiciliacionRequest.Periodicidad));
        
    }

    [Test]
    public async Task CreateAsync_ClienteNotFoundException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid))
            .ReturnsAsync((Cliente)null);
        
        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => 
            await _domiciliacionService.CreateAsync(domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado ningún cliente con guid: {domiciliacionRequest.ClienteGuid}"));
    }

    [Test]
    public async Task CreateAsync_CuentaNotFoundException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync((Cuenta)null);
        
        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => 
            await _domiciliacionService.CreateAsync(domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}"));
    }

    [Test]
    public async Task CreateAsync_CuentaIbanException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };

        var cuenta = new Cuenta
        {
            Guid = "cuenta-123",
            ClienteGuid = "cliente-456", 
            Saldo = 1000
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta);
        
        var ex = Assert.ThrowsAsync<CuentaIbanException>(async () => 
            await _domiciliacionService.CreateAsync(domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {domiciliacionRequest.ClienteGuid}"));
    }

    [Test]
    public async Task CreateAsync_SaldoCuentaInsuficientException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 2000, 
            Periodicidad = "Mensual",
            Activa = true
        };

        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };

        var cuenta = new Cuenta
        {
            Guid = "cuenta-123",
            ClienteGuid = "cliente-123",
            Saldo = 1000 
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(domiciliacionRequest.ClienteGuid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta);
        
        var ex = Assert.ThrowsAsync<SaldoCuentaInsuficientException>(async () => 
            await _domiciliacionService.CreateAsync(domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €"));
    }

    [Test]
    public async Task CreateAsync_PeriodicidadNotValidException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Anual", 
            Activa = true
        };
        
        var ex = Assert.ThrowsAsync<PeriodicidadNotValidException>(async () => 
            await _domiciliacionService.CreateAsync(domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo("Periodicidad no válida: Anual"));
    }
    

    [Test]
    public async Task DesactivateDomiciliacion()
    {
        var domiciliacionGuid = "domiciliacion-123";
        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            Activa = true,
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual
        };

        _domiciliacionCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Domiciliacion>>()).FirstOrDefaultAsync())
            .ReturnsAsync(domiciliacion);
        _domiciliacionCollection.Setup(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Domiciliacion>>(), It.IsAny<Domiciliacion>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _memoryCache.Setup(mc => mc.Remove(It.IsAny<string>()));
        _redisDatabase.Setup(rd => rd.KeyDeleteAsync(It.IsAny<string>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
        _domiciliacionCollection.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Domiciliacion>>(), It.IsAny<Domiciliacion>()), Times.Once);
        _memoryCache.Verify(mc => mc.Set(It.IsAny<string>(), It.IsAny<Domiciliacion>(), It.IsAny<TimeSpan>()), Times.Once);
        _redisDatabase.Verify(rd => rd.StringSetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
       
    }

    [Test]
    public async Task DesactivateDomiciliacion_InMemoryCache()
    {
        var domiciliacionGuid = "domiciliacion-123";
        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            Activa = true,
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual
        };

        _memoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out domiciliacion)).Returns(true);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
        _domiciliacionCollection.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Domiciliacion>>(), It.IsAny<Domiciliacion>()), Times.Once);
        _redisDatabase.Verify(rd => rd.KeyDeleteAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task DesactivateDomiciliacion_Redis()
    {
        var domiciliacionGuid = "domiciliacion-123";
        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            Activa = true,
            ClienteGuid = "cliente-123",
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = Periodicidad.Mensual
        };

        var serializedDom = JsonSerializer.Serialize(domiciliacion);

        _redisDatabase.Setup(rd => rd.StringGetAsync(It.IsAny<string>())).ReturnsAsync(serializedDom);
        _redisDatabase.Setup(rd => rd.KeyDeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
        _domiciliacionCollection.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Domiciliacion>>(), It.IsAny<Domiciliacion>()), Times.Once);
        _memoryCache.Verify(mc => mc.Set(It.IsAny<string>(), It.IsAny<Domiciliacion>(), It.IsAny<TimeSpan>()), Times.Once);
       
    }

    [Test]
    public async Task DesactivateDomiciliacionAsync_MovimientoDeserialiceException()
    {
        var domiciliacionGuid = "domiciliacion-123";

        _redisDatabase.Setup(rd => rd.StringGetAsync(It.IsAny<string>())).ReturnsAsync("invalid-serialized-data");
        
        var ex = Assert.ThrowsAsync<MovimientoDeserialiceException>(async () =>
            await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid));

        Assert.That(ex.Message, Is.EqualTo("Error al deserializar domiciliación desde Redis"));
    }*/
    
}

