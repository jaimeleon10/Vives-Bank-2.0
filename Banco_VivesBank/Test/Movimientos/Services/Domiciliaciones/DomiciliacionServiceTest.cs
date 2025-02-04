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
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Dto;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Test.Movimientos.Services.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionService))]
public class DomiciliacionServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private MongoDbContainer _mongoDbContainer;
    private GeneralDbContext _dbContext;
    private IMongoCollection<Domiciliacion> _domiciliacionCollection;
    private DomiciliacionService _domiciliacionService;
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
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();
        await _postgreSqlContainer.StartAsync();
        
        var dbOptions = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;
        _dbContext = new GeneralDbContext(dbOptions);
        await _dbContext.Database.EnsureCreatedAsync();
        
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
    
    [SetUp]
    public async Task CleanDatabase()
    {
        // Limpiar la colección de domiciliaciones antes de cada test
        await _domiciliacionCollection.DeleteManyAsync(Builders<Domiciliacion>.Filter.Empty);
    }
        
    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
        
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
        
        Assert.That(result.Count(), Is.EqualTo(2));
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
        
        Assert.That(result.Count(), Is.EqualTo(1));
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
        
        Assert.That(result.Count(), Is.EqualTo(2));
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
        
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task CreateAsync()
    {
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var clienteModel = new Banco_VivesBank.Cliente.Models.Cliente()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };

        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban123",
            Saldo = 2000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);
        _clienteService.Setup(service => service.GetClienteModelByGuid(clienteAuth.Guid)).ReturnsAsync(clienteModel);
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var result = await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest);
        
        Assert.That(result.ClienteGuid, Is.EqualTo("cliente-guid"));
        Assert.That(result.Acreedor, Is.EqualTo("Acreedor1"));
        Assert.That(result.IbanEmpresa, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result.IbanCliente, Is.EqualTo("ES9876543210987654321098"));
        Assert.That(result.Importe, Is.EqualTo(200));
        Assert.That(result.Periodicidad, Is.EqualTo("Mensual"));
        Assert.That(result.Activa, Is.True);
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
        
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync((CuentaResponse)null!);

        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {domiciliacionRequest.IbanCliente}"));
    }
    
    [Test]
    public void CreateAsync_CuentaNoPertenecienteACliente()
    {
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var clienteModel = new Banco_VivesBank.Cliente.Models.Cliente()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };

        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321000",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban123",
            Saldo = 50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "",
            ProductoGuid = "producto-guid"
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);
        _clienteService.Setup(service => service.GetClienteModelByGuid(clienteAuth.Guid)).ReturnsAsync(clienteModel);
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var ex = Assert.ThrowsAsync<CuentaIbanException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"El iban con guid: {domiciliacionRequest.IbanCliente} no pertenece a ninguna cuenta del cliente con guid: {clienteModel.Guid}"));
    }
    
    [Test]
    public void CreateAsync_CuentaSaldoInsuficiente()
    {
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var clienteModel = new Banco_VivesBank.Cliente.Models.Cliente()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };

        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "Mensual",
            Activa = true
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban123",
            Saldo = 50,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);
        _clienteService.Setup(service => service.GetClienteModelByGuid(clienteAuth.Guid)).ReturnsAsync(clienteModel);
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var ex = Assert.ThrowsAsync<SaldoCuentaInsuficientException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"Saldo insuficiente en la cuenta con guid: {cuentaResponse.Guid} respecto al importe de {domiciliacionRequest.Importe} €"));
    }
    
    [Test]
    public void CreateAsync_PeriodicidadNoValida()
    {
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var clienteModel = new Banco_VivesBank.Cliente.Models.Cliente()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };

        var domiciliacionRequest = new DomiciliacionRequest
        {
            Acreedor = "Acreedor1",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 200,
            Periodicidad = "",
            Activa = true
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban123",
            Saldo = 2000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);
        _clienteService.Setup(service => service.GetClienteModelByGuid(clienteAuth.Guid)).ReturnsAsync(clienteModel);
        _cuentaService.Setup(service => service.GetByIbanAsync(domiciliacionRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        
        var ex = Assert.ThrowsAsync<PeriodicidadNotValidException>(async () => await _domiciliacionService.CreateAsync(userAuth, domiciliacionRequest));
        
        Assert.That(ex.Message, Is.EqualTo($"Periodicidad no válida: {domiciliacionRequest.Periodicidad}"));
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
    
    [Test]
    public async Task DesactivateDomiciliacionAsync_BBDD()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new UserEntity
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };
        
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var domiciliacion = new Domiciliacion
        {
            Guid = "domiciliacion-guid-test",
            ClienteGuid = "cliente-guid",
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            Activa = true,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow
        };
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);

        var cacheKey = "Domiciliaciones:" + domiciliacion.Guid;
        _memoryCache.Remove(cacheKey);
        
        _redisDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacion.Guid);

        Assert.That(result, Is.Not.Null, "El resultado no debe ser nulo");
        Assert.That(result.Activa, Is.False, "La domiciliación debe estar desactivada");
        
    }
    
    [Test]
    public async Task DesactivateMyDomiciliacion_CacheMemoria()
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
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };

        await _domiciliacionCollection.InsertOneAsync(domiciliacion);
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);

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
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.True, "La domiciliación debe haberse eliminado de Redis");
    }

    [Test] public async Task DesactivateMyDomiciliacionAsync_BBDD()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new UserEntity
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };
        
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var domiciliacion = new Domiciliacion
        {
            Guid = "domiciliacion-guid",
            ClienteGuid = "cliente-guid",
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            Activa = true,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow
        };
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);

        var cacheKey = "Domiciliaciones:" + domiciliacion.Guid;
        _memoryCache.Remove(cacheKey);
        
        _redisDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);

        var result = await _domiciliacionService.DesactivateDomiciliacionAsync(domiciliacion.Guid);

        Assert.That(result, Is.Not.Null, "El resultado no debe ser nulo");
        Assert.That(result.Activa, Is.False, "La domiciliación debe estar desactivada");
        
    }
    
    [Test] public async Task DesactivateMyDomiciliacionAsync_MovimientoNoPertenecienteACliente()
    {
        var cliente = new ClienteEntity
        {
            Guid = "cliente-guid",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new UserEntity
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente,
            },
        };
        
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid", 
            Username = "usuario", 
            Password = "password", 
            Role = Banco_VivesBank.User.Models.Role.Cliente,
        };
        
        var clienteAuth = new ClienteResponse()
        {
            Guid = "",
            Dni = "12345678Z",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Direccion = new Direccion
            {
                Calle = "calle",
                CodigoPostal = "12345",
                Letra = "A",
                Numero = "10",
                Piso = "1"
            },
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "usuario",
                Password = "password",
                Role = Banco_VivesBank.User.Models.Role.Cliente.ToString(),
            },
        };
        
        var domiciliacion = new Domiciliacion
        {
            Guid = "domiciliacion-guid",
            ClienteGuid = "cliente-guid",
            Acreedor = "Acreedor Test",
            IbanEmpresa = "IBAN123",
            IbanCliente = "IBAN456",
            Importe = 100.0,
            Periodicidad = Periodicidad.Mensual,
            Activa = true,
            FechaInicio = DateTime.UtcNow,
            UltimaEjecucion = DateTime.UtcNow
        };
        await _domiciliacionCollection.InsertOneAsync(domiciliacion);

        var cacheKey = "Domiciliaciones:" + domiciliacion.Guid;
        _memoryCache.Remove(cacheKey);
        
        _redisDatabase.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteAuth);

        var ex = Assert.ThrowsAsync<MovimientoNoPertenecienteAlUsuarioAutenticadoException>(async () => await _domiciliacionService.DesactivateMyDomiciliacionAsync(userAuth, domiciliacion.Guid));
        
        Assert.That(ex.Message, Is.EqualTo($"La domiciliación con guid {domiciliacion.Guid} no pertenece al cliente autenticado con guid {clienteAuth.Guid} y no puede ser desactivada"));
        
    }

}