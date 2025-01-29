using System.Text.Json;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using NSubstitute;
using StackExchange.Redis;
using Testcontainers.MongoDb;


namespace Test.Movimientos.Services.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionService))]
public class DomiciliacionServiceTest
{
    private MongoDbContainer _mongoDbContainer;
    private IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private DomiciliacionService _domiciliacionService;
    private GeneralDbContext _dbContext;
    private Mock<IClienteService> _clienteServiceMock;
    private Mock<ICuentaService> _cuentaServiceMock;
    private Mock<IMovimientoService> _movimientoServiceMock;
    private Mock<IOptions<MovimientosMongoConfig>> _mongoConfigMock;
    private Mock<ILogger<DomiciliacionService>> _loggerMock;
    private Mock<IMemoryCache> _memoryCacheMock;
    private Mock<IConnectionMultiplexer> _redisMock;
    private Mock<IDatabase> _databaseMock;

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
        
        _clienteServiceMock = new Mock<IClienteService>();
        _cuentaServiceMock = new Mock<ICuentaService>();
        _movimientoServiceMock = new Mock<IMovimientoService>();
        _mongoConfigMock = new Mock<IOptions<MovimientosMongoConfig>>();
        _loggerMock = new Mock<ILogger<DomiciliacionService>>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        
        _mongoConfigMock.Setup(x => x.Value).Returns(new MovimientosMongoConfig
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb",
            DomiciliacionesCollectionName = "domiciliaciones"
        });
        
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        
        _domiciliacionService = new DomiciliacionService(
            _mongoConfigMock.Object,    
            _loggerMock.Object,         
            _clienteServiceMock.Object,  
            _cuentaServiceMock.Object,  
            _dbContext, 
            _redisMock.Object,
            _memoryCacheMock.Object,
            _movimientoServiceMock.Object
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
        
        Assert.That(result.Count, Is.EqualTo(2));
        
    }
    
    
    [Test]
    public async Task GetByGuidAsync_NoEstaEnCache()
    {
        var domiciliacionGuid = "test-guid";
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
        
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        if (!memoryCache.TryGetValue(domiciliacionGuid, out Domiciliacion cachedDomiciliacion))
        {
            cachedDomiciliacion = null;
        }
        
        var serialized = JsonSerializer.Serialize("test");
        _databaseMock.Setup(db => db.StringGetAsync("TestNotFound")).ReturnsAsync(RedisValue.Null);
        
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(domiciliacionGuid));
    }

    [Test]
    public async Task GetByGuidAsync_MemoriaCache()
    {
        var domiciliacionGuid = "test-guid";
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

        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<Domiciliacion>.IsAny)).Returns(true);
        _memoryCacheMock.Setup(m => m.Get(It.IsAny<string>())).Returns(domiciliacion);
        
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(domiciliacionGuid));
    }

    [Test]
    public async Task GetByGuidAsync_Redis()
    {
        var domiciliacionGuid = "test-guid";
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
        var redisValue = JsonSerializer.Serialize(domiciliacion);

        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<Domiciliacion>.IsAny)).Returns(false);
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        //_databaseMock.Setup(db => db.StringGetAsync(It.IsAny<string>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null);
        
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(domiciliacionGuid));
    }

    [Test]
    public async Task GetByGuidAsync_NotFound()
    {
        var domiciliacionGuid = "test-guid";

        _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<Domiciliacion>.IsAny)).Returns(false);
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        //_databaseMock.Setup(db => db.StringGetAsync(It.IsAny<string>())).ReturnsAsync(RedisValue.Null);
        
        var result = await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Null);
    }
}
