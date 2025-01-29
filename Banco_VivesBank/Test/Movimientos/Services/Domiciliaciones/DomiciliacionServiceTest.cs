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
using StackExchange.Redis;
using Testcontainers.MongoDb;

/*
namespace Test.Movimientos.Services.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionService))]
public class DomiciliacionServiceTest
{
    private MongoDbContainer _mongoDbContainer;
    private IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private DomiciliacionService _domiciliacionService;

    private Mock<IClienteService> _clienteServiceMock;
    private Mock<ICuentaService> _cuentaServiceMock;
    private Mock<IMovimientoService> _movimientoServiceMock;
    private Mock<IOptions<MovimientosMongoConfig>> _mongoConfigMock;
    private Mock<ILogger<DomiciliacionService>> _loggerMock;

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
        
        _mongoConfigMock.Setup(x => x.Value).Returns(new MovimientosMongoConfig
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb",
            DomiciliacionesCollectionName = "domiciliaciones"
        });
        
        _domiciliacionService = new DomiciliacionService(
            _mongoConfigMock.Object,    
            _loggerMock.Object,         
            _clienteServiceMock.Object,  
            _cuentaServiceMock.Object,  
            mongoDatabase,               
            Mock.Of<IConnectionMultiplexer>(), 
            Mock.Of<IMemoryCache>(),          
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

        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 2,
            SortBy = "guid",
            Direction = "ASC"
        };
        
        var result = await _domiciliacionService.GetAllAsync(pageRequest);
        
        Assert.That(result.Content.Count, Is.EqualTo(2)); 
        Assert.That(result.PageNumber, Is.EqualTo(0)); 
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalElements, Is.EqualTo(2)); 
        Assert.That(result.TotalPages, Is.EqualTo(1)); 
        Assert.That(result.Empty, Is.False); 
        Assert.That(result.First, Is.True);
        Assert.That(result.Last, Is.True);
    }
}*/

