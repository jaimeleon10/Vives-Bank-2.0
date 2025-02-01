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
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.User.Models;
using NSubstitute;

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
    
    /*
    
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
    
    [Test]
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
        
        var redisValue = JsonSerializer.Serialize<Domiciliacion>(domiciliacion);
        
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
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };

        var cuenta = new Cuenta
        {
            Guid = "cuenta-123",
            Iban = "ES9876543210987654321098",
            Saldo = 1000
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta.ToResponseFromModel());

        // Act
        var result = await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Acreedor, Is.EqualTo(domiciliacionRequest.Acreedor));
        Assert.That(result.IbanCliente, Is.EqualTo(domiciliacionRequest.IbanCliente));
        Assert.That(result.Importe, Is.EqualTo(domiciliacionRequest.Importe));
        Assert.That(result.Periodicidad, Is.EqualTo(domiciliacionRequest.Periodicidad));
        Assert.That(result.Activa, Is.EqualTo(domiciliacionRequest.Activa));
    }*/

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

    /*
    [Test]
    public async Task CreateAsync_CuentaNotFoundException()
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
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync((CuentaResponse?)null);
        
        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}"));
    }

    [Test]
    public async Task CreateAsync_CuentaIbanException()
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

        var user = new User
        {
            Guid = "user-123",
            Username = "testuser"
        };

        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente",
            User = user,  // Asegúrate de que el 'User' esté correctamente asignado
            Cuentas = new List<Cuenta>()
        };

        // Añadir el cliente a la base de datos
        _dbContext.Clientes.Add(cliente.ToEntityFromModel());
        await _dbContext.SaveChangesAsync();

        var cuenta = new Cuenta
        {
            Guid = "cuenta-123",
            Iban = "ES9876543210987654321098",  // Iban que coincide con el de la solicitud
            Saldo = 1000
        };

        // Asegurarse de que la lista de cuentas del cliente se rellene
        cliente.Cuentas = new List<Cuenta>{cuenta};

        // Configuración de mocks
        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync(cliente);

        // Mock para `GetByIbanAsync` que devuelve la cuenta correcta
        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta.ToResponseFromModel()); // Asegúrate de que este método devuelve un valor válido
    
        // Act y Assert
        var ex = Assert.ThrowsAsync<CuentaIbanException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));

        // Verificación del mensaje de la excepción
        Assert.That(ex.Message, Is.EqualTo($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {cliente.Guid}"));
    }


    [Test]
    public async Task CreateAsync_SaldoCuentaInsuficientException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
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
            Saldo = 1000 
        };

        _clienteService.Setup(c => c.GetClienteModelByGuid(cliente.Guid))
            .ReturnsAsync(cliente);

        _cuentaService.Setup(c => c.GetByIbanAsync(domiciliacionRequest.IbanCliente))
            .ReturnsAsync(cuenta.ToResponseFromModel);
        
        var ex = Assert.ThrowsAsync<SaldoCuentaInsuficientException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));
        Assert.That(ex.Message, Is.EqualTo($"Saldo insuficiente en la cuenta con guid: {cuenta.Guid} respecto al importe de {domiciliacionRequest.Importe} €"));
    }

    [Test]
    public async Task CreateAsync_PeriodicidadNotValidException()
    {
        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Anual", 
            Activa = true
        };
        
        var cliente = new Cliente
        {
            Guid = "cliente-123",
            Nombre = "Test Cliente"
        };
        
        var ex = Assert.ThrowsAsync<PeriodicidadNotValidException>(async () => 
            await _domiciliacionService.CreateAsync(cliente.User, domiciliacionRequest));
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

        _memoryCache.Remove(It.IsAny<string>());
        _redisDatabase.Setup(rd => rd.KeyDeleteAsync(It.IsAny<string>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
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

        _memoryCache.TryGetValue(It.IsAny<string>(), out domiciliacion).Returns(true);
        
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
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

        _redisDatabase.Setup(rd => rd.StringGetAsync(It.IsAny<string>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(serializedDom);
        _redisDatabase.Setup(rd => rd.KeyDeleteAsync(It.IsAny<string>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
    
        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid);
    
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Activa, Is.False);
    }

    [Test]
    public async Task DesactivateDomiciliacionAsync_MovimientoDeserialiceException()
    {
        var domiciliacionGuid = "domiciliacion-123";

        _redisDatabase.Setup(rd => rd.StringGetAsync(It.Is<string>(s => s == domiciliacionGuid), It.IsAny<CommandFlags>()))
            .ReturnsAsync("invalid-serialized-data");
    
        var ex = Assert.ThrowsAsync<MovimientoDeserialiceException>(async () =>
            await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacionGuid));

        Assert.That(ex.Message, Is.EqualTo("Error al deserializar domiciliación desde Redis"));
    }*/

}