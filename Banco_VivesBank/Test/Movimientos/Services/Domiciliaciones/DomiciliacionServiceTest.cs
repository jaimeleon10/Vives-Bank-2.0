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
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Cuenta.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using StackExchange.Redis;
using Testcontainers.MongoDb;
using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Models;

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
        Assert.That(result.Count(), Is.EqualTo(2));
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
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
        
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
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };

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
    public async Task CreateAsync_Success()
    {
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 100, Periodicidad = "Mensual", Activa = true };
        
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
        
        _movimientoService.Setup(service => service.CreateAsync(It.IsAny<MovimientoRequest>())).Returns(Task.CompletedTask);
        
        var result = await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest);
        
        Assert.That(result, Is.Not.Null);
    }
    
    

    [Test]
    public async Task CreateAsync_ClienteNotFoundException()
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
        
        var cliente = new Cliente
        {
            Guid = "clienteguidnoexistente",
            Nombre = "Test Cliente"
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync((Cliente)null);
        
        var ex = Assert.ThrowsAsync<ClienteNotFoundException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado ningún cliente autenticado"));
    }
    

    [Test]
    public async Task CreateAsync_CuentaNotFoundException()
    {
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 100, Periodicidad = "Mensual", Activa = true };
        
        var clienteResponse = new ClienteResponse { Guid = "cliente-1", Nombre = "Cliente 1", Apellidos = "Apellido1" };
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync((CuentaResponse)null);

        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}"));
    }

    [Test]
    public async Task CreateAsync_CuentaIbanMismatchException()
    {
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
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
    public async Task CreateAsync_SaldoInsuficienteException()
    {
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
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
    public async Task CreateAsync_PeriodicidadNotValidException()
    {
        var userAuth = new User { Guid = "cliente-1", Username = "usuario123" };
        var domiciliacionRequest = new DomiciliacionRequest { IbanCliente = "ES1234567890123456789012", Importe = 100, Periodicidad = "Anual", Activa = true };

       
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
        
        var ex = Assert.ThrowsAsync<PeriodicidadNotValidException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"Periodicidad no válida: {domiciliacionRequest.Periodicidad}"));
    }
    
    [Test]
    public async Task DesactivateDomiciliacion_CacheMemoria()
    {
       
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;

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
            UltimaEjecucion = DateTime.UtcNow
        };
        
        _memoryCache.Set(cacheKey, domiciliacion, TimeSpan.FromMinutes(30));
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True);  
    }
    
    /*[Test]
    public async Task DesactivateDomiciliacion()
    {
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;

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
            UltimaEjecucion = DateTime.UtcNow
        };
        
        _domiciliacionCollection.Setup(coll => coll.Find(It.IsAny<FilterDefinition<Domiciliacion>>())
            .FirstOrDefaultAsync()).ReturnsAsync(domiciliacion);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);  
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True); 
        Assert.That(await _redisDatabase.KeyExistsAsync(cacheKey), Is.True);  
    }
    
    [Test]
    public async Task DesactivateDomiciliacionAsync_DomiciliacionNotFound()
    {
       
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;

        
        _domiciliacionCollection.Setup(coll => coll.Find(It.IsAny<FilterDefinition<Domiciliacion>>())
            .FirstOrDefaultAsync()).ReturnsAsync((Domiciliacion?)null);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Null); 
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.False); 
        Assert.That(await _redisDatabase.KeyExistsAsync(cacheKey), Is.False); 
    }
    
  
    
    [Test]
    public async Task DesactivateMyDomiciliacionAsync_NotFounD()
    {
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;
        
        _domiciliacionCollection.Setup(coll => coll.Find(It.IsAny<FilterDefinition<Domiciliacion>>())
            .FirstOrDefaultAsync()).ReturnsAsync((Domiciliacion?)null);
        
        var result = await _domiciliacionService.DesactivateMyDomiciliacionAsync(new User { Username = "usuario_test" }, domiciliacionGuid);
        
        Assert.That(result, Is.Null); 
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.False);  
        Assert.That(await _redisDatabase.KeyExistsAsync(cacheKey), Is.False);  
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacionAsync_UserDoesNotOwnDomiciliacionException()
    {
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;

        var domiciliacion = new Domiciliacion
        {
            Guid = domiciliacionGuid,
            ClienteGuid = "cliente-guid-diferente", 
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow
        };

        var userAuth = new User
        {
            Username = "usuario_test",
            Guid = "cliente-guid"  
        };
        
        _domiciliacionCollection.Setup(coll => coll.Find(It.IsAny<FilterDefinition<Domiciliacion>>())
            .FirstOrDefaultAsync()).ReturnsAsync(domiciliacion);
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth))
            .ReturnsAsync(new ClienteResponse() { Guid = "cliente-guid" });
        
        var ex = Assert.ThrowsAsync<MovimientoNoPertenecienteAlUsuarioAutenticadoException>(async () =>
            await _domiciliacionService.DesactivateMyDomiciliacionAsync(userAuth, domiciliacionGuid));
    
        Assert.That(ex.Message, Is.EqualTo($"La domiciliación con guid {domiciliacionGuid} no pertenece al cliente autenticado con guid {userAuth.Guid} y no puede ser desactivada"));
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacionAsync_UpdatesCache()
    {
      
        var domiciliacionGuid = "domiciliacion-guid";
        var cacheKey = "Domiciliaciones" + domiciliacionGuid;

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
            UltimaEjecucion = DateTime.UtcNow
        };

        var userAuth = new User
        {
            Username = "usuario_test",
            Guid = "cliente-guid" 
        };
        
        _domiciliacionCollection.Setup(coll => coll.Find(It.IsAny<FilterDefinition<Domiciliacion>>())
            .FirstOrDefaultAsync()).ReturnsAsync(domiciliacion);
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth))
            .ReturnsAsync(new ClienteResponse() { Guid = "cliente-guid" });
        
        var result = await _domiciliacionService.DesactivateMyDomiciliacionAsync(userAuth, domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);  
        
        Assert.That(_memoryCache.TryGetValue(cacheKey, out Domiciliacion updatedDomiciliacion), Is.True);
        Assert.That(updatedDomiciliacion.Activa, Is.False); 

        var redisValue = await _redisDatabase.StringGetAsync(cacheKey);
        var deserializedDomiciliacion = JsonSerializer.Deserialize<Domiciliacion>(redisValue);
        Assert.That(deserializedDomiciliacion?.Activa, Is.False);  
    }*/
    
    
}