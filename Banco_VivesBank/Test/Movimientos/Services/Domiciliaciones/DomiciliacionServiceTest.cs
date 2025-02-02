using System.Text.Json;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using StackExchange.Redis;
using Testcontainers.MongoDb;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.User.Dto;

namespace Test.Movimientos.Services.Domiciliaciones;

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

        var cacheKey = "Domiciliaciones:" + "domiciliacion-guid";
        var simulatedRedisValue = JsonSerializer.Serialize(new Domiciliacion
        {
            Guid = "domiciliacion-guid",
            ClienteGuid = "cliente-guid",
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow,
            Activa = true
        });
        
        _redisDatabase.Setup(db => db.StringGetAsync(cacheKey, It.IsAny<CommandFlags>())).ReturnsAsync(simulatedRedisValue);

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
    [Order(1)]
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
        Assert.That(result.Count(), Is.EqualTo(3));
    }
    
    [Test]
    public async Task GetByClienteGuidAsync()
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
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor2",
            IbanEmpresa = "ES2345678901234567890123",
            IbanCliente = "ES8765432109876543210987",
            Importe = 300,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };
        
        await _domiciliacionCollection.InsertManyAsync(new List<Domiciliacion> { domiciliacion1, domiciliacion2 });
        
        var result = await _domiciliacionService.GetByClienteGuidAsync("cliente-1");
        
        Assert.That(result.Count(), Is.EqualTo(3));
        Assert.That(result.First().Acreedor, Is.EqualTo("Acreedor1"));
        Assert.That(result.Last().Acreedor, Is.EqualTo("Acreedor2"));
    }
    
    [Test]
    public async Task GetByClienteGuidAsync_SinDomiciliaciones()
    {
       
        var result = await _domiciliacionService.GetByClienteGuidAsync("cliente-3");
        Assert.That(result.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public async Task GetByClienteGuidAsync_DomiciliacionesInactivas()
    {
        var domiciliacionInactiva = new Domiciliacion
        {
            ClienteGuid = "cliente-1",
            Acreedor = "AcreedorInactivo",
            IbanEmpresa = "ES2345678901234567890123",
            IbanCliente = "ES8765432109876543210987",
            Importe = 100,
            Periodicidad = Periodicidad.Mensual,
            Activa = false
        };
        
        await _domiciliacionCollection.InsertOneAsync(domiciliacionInactiva);
        
        var result = await _domiciliacionService.GetByClienteGuidAsync("cliente-1");
        
        Assert.That(result.Count(), Is.EqualTo(4));
    }
    
    [Test]
    public async Task GetMyDomiciliaciones_Success()
    {
        var userAuth = new Banco_VivesBank.User.Models.User { Guid = "cliente-1", Username = "usuario123" };
        
        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-1",
            Nombre = "Cliente 1",
            Apellidos = "Apellido1",
            Email = "cliente1@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Username = "usuario123",
                Password = "pass"
            },
            CreatedAt = "2025-02-02",
            UpdatedAt = "2025-02-02",
            IsDeleted = false
        };

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
            ClienteGuid = "cliente-1",
            Acreedor = "Acreedor2",
            IbanEmpresa = "ES2345678901234567890123",
            IbanCliente = "ES8765432109876543210987",
            Importe = 300,
            Periodicidad = Periodicidad.Mensual,
            Activa = true
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        await _domiciliacionCollection.InsertManyAsync(new List<Domiciliacion> { domiciliacion1, domiciliacion2 });
        
        var result = await _domiciliacionService.GetMyDomiciliaciones(userAuth);
        
        Assert.That(result.Count(), Is.EqualTo(6));
        Assert.That(result.First().Acreedor, Is.EqualTo("Acreedor1"));
        Assert.That(result.Last().Acreedor, Is.EqualTo("Acreedor2"));
    }
    
    [Test]
    public async Task GetMyDomiciliaciones_NoDomiciliaciones()
    {
        var userAuth = new Banco_VivesBank.User.Models.User { Guid = "cliente-1", Username = "usuario123" };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-1",
            Nombre = "Cliente 1",
            Apellidos = "Apellido1",
            Email = "cliente1@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Username = "cliente1",
                Password = "pass1"
            },
            CreatedAt = "2025-02-02",
            UpdatedAt = "2025-02-02",
            IsDeleted = false
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        var result = await _domiciliacionService.GetMyDomiciliaciones(userAuth);
        
        Assert.That(result.Count(), Is.EqualTo(4));
    }
    
    [Test]
    public void CreateAsync_ClienteNotFoundException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };
        
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = "clienteguidnoexistente",
            Nombre = "Test Cliente"
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync((Banco_VivesBank.Cliente.Models.Cliente)null);
        
        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado ningún cliente autenticado"));
    }

    [Test]
    public void CreateAsync_CuentaNotFoundException()
    {
        var userAuth = new Banco_VivesBank.User.Models.User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 100, Periodicidad = "Mensual", Activa = true };
        
        var clienteResponse = new ClienteResponse { Guid = "cliente-1", Nombre = "Cliente 1", Apellidos = "Apellido1" };
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync((CuentaResponse)null);

        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}"));
    }

    [Test]
    public void CreateAsync_CuentaIbanMismatchException()
    {
        var userAuth = new Banco_VivesBank.User.Models.User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 100, Periodicidad = "Mensual", Activa = true };
        
        var clienteResponse = new ClienteResponse { Guid = "cliente-1", Nombre = "Cliente 1", Apellidos = "Apellido1" };
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-1",
            Iban = "ES1234567890123456789012",
            ClienteGuid = "cliente-2", 
            ProductoGuid = "producto-1",
            Saldo = 1000,
            CreatedAt = DateTime.UtcNow.ToString(),
            UpdatedAt = DateTime.UtcNow.ToString(),
            IsDeleted = false
        };
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var ex = Assert.ThrowsAsync<CuentaIbanException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {clienteResponse.Guid}"));
    }

    [Test]
    public void CreateAsync_SaldoInsuficienteException()
    {
        var userAuth = new Banco_VivesBank.User.Models.User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 1000, Periodicidad = "Mensual", Activa = true };
        
        var clienteResponse = new ClienteResponse { Guid = "cliente-1", Nombre = "Cliente 1", Apellidos = "Apellido1" };
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var cuentaResponse = new CuentaResponse 
        { 
            Guid = "cuenta-1", 
            Iban = "ES1234567890123456789012", 
            Saldo = 500,  
            ClienteGuid = "cliente-1", 
            ProductoGuid = "producto-1", 
            CreatedAt = DateTime.UtcNow.ToString(), 
            UpdatedAt = DateTime.UtcNow.ToString(), 
            IsDeleted = false 
        };
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var ex = Assert.ThrowsAsync<SaldoCuentaInsuficientException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"Saldo insuficiente en la cuenta con guid: {cuentaResponse.Guid} respecto al importe de {domiciliacionRequest.Importe} €"));
    }
    
    [Test]
    public async Task DesactivateDomiciliacion_CacheMemoria()
    {
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones:" + domiciliacionGuid;

        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente-guid",
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow,
            Activa = false
        };

        await _domiciliacionCollection.InsertOneAsync(domiciliacion);

        _memoryCache.Set(cacheKey, domiciliacion, TimeSpan.FromMinutes(30));

        var dbDomiciliacion = await _domiciliacionCollection.Find(dom => dom.Guid == domiciliacionGuid).FirstOrDefaultAsync();
        Assert.That(dbDomiciliacion, Is.Not.Null, "La domiciliación debe existir en la base de datos");
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True, "La domiciliación debe estar en la caché antes de desactivarla");

        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        Assert.That(result, Is.Not.Null, "El resultado no debe ser nulo");
        Assert.That(result.Activa, Is.False, "La domiciliación debe estar desactivada");
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True, "La domiciliación debe haberse eliminado de la caché");

        var updatedDomiciliacion = await _domiciliacionCollection.Find(dom => dom.Guid == domiciliacionGuid).FirstOrDefaultAsync();
        Assert.That(updatedDomiciliacion, Is.Not.Null, "La domiciliación debe existir en la base de datos después de la desactivación");
        Assert.That(updatedDomiciliacion.Activa, Is.False, "La domiciliación debe estar desactivada en la base de datos");

        var redisCacheValue = await _redisDatabase.Object.StringGetAsync(cacheKey);
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.False, "La domiciliación debe haberse eliminado de Redis");
    }
}