using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.User.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using StackExchange.Redis;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Role = Banco_VivesBank.User.Models.Role;

namespace Test.Movimientos.Services.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoService))]
public class MovimientoServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private MongoDbContainer _mongoDbContainer;
    private GeneralDbContext _dbContext;
    private IMongoCollection<Movimiento> _movimientoCollection;
    private MovimientoService _movimientoService;
    private Mock<IClienteService> _clienteService;
    private Mock<ICuentaService> _cuentaService;
    private Mock<ITarjetaService> _tarjetaService;
    private Mock<IOptions<MovimientosMongoConfig>> _mongoConfigMock;
    private Mock<ILogger<MovimientoService>> _loggerMock;
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
        _movimientoCollection = mongoDatabase.GetCollection<Movimiento>("movimientos");
        
        _clienteService = new Mock<IClienteService>();
        _cuentaService = new Mock<ICuentaService>();
        _tarjetaService = new Mock<ITarjetaService>();
        _mongoConfigMock = new Mock<IOptions<MovimientosMongoConfig>>();
        _loggerMock = new Mock<ILogger<MovimientoService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _redis = new Mock<IConnectionMultiplexer>();
        _redisDatabase = new Mock<IDatabase>();

        _mongoConfigMock.Setup(x => x.Value).Returns(new MovimientosMongoConfig
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb",
            MovimientosCollectionName = "movimientos"
        });

        _redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDatabase.Object);

        _movimientoService = new MovimientoService(
            _mongoConfigMock.Object,
            _loggerMock.Object,
            _clienteService.Object,
            _cuentaService.Object,
            _dbContext,
            _tarjetaService.Object,
            _redis.Object,
            _memoryCache
        );
    }

    [SetUp]
    public async Task CleanDatabase()
    {
        // Limpiar la colección de movimientos antes de cada test
        await _movimientoCollection.DeleteManyAsync(Builders<Movimiento>.Filter.Empty);
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
    [Order(1)]
    public async Task GetAllAsync()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };

        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = "nombre-comercio",
            Importe = 200,
            NumeroTarjeta = "4532111122223333"
        };
        
        var movimiento1 = new Movimiento
        {
            Guid = "movimiento1-guid",
            ClienteGuid = "cliente-1",
            Transferencia = transferencia
        };

        var movimiento2 = new Movimiento
        {
            Guid = "movimiento2-guid",
            ClienteGuid = "cliente-2",
            PagoConTarjeta = pagoConTarjeta
        };

        await _movimientoCollection.InsertManyAsync(new List<Movimiento> { movimiento1, movimiento2 });
        
        var result = await _movimientoService.GetAllAsync();
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Guid, Is.EqualTo("movimiento1-guid"));
        Assert.That(result.First().ClienteGuid, Is.EqualTo("cliente-1"));
        Assert.That(result.First().Transferencia.Importe, Is.EqualTo(transferencia.Importe));
        Assert.That(result.Last().Guid, Is.EqualTo("movimiento2-guid"));
        Assert.That(result.Last().ClienteGuid, Is.EqualTo("cliente-2"));
        Assert.That(result.Last().PagoConTarjeta.NumeroTarjeta, Is.EqualTo(pagoConTarjeta.NumeroTarjeta));
    }

    [Test]
    [Order(2)]
    public async Task GetByGuidAsync()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };
        
        var movimiento = new Movimiento
        {
            Guid = "movimiento-guid",
            ClienteGuid = "cliente-guid",
            Transferencia = transferencia
        };
        
        await _movimientoCollection.InsertOneAsync(movimiento);
        
        var cacheKey = "Movimientos:" + movimiento.Guid;
        _memoryCache.Remove(cacheKey);
        
        var result = await _movimientoService.GetByGuidAsync(movimiento.Guid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.False, "El movimiento no debe estar en la caché en memoria");
        var redisCacheValue = await _redisDatabase.Object.StringGetAsync(cacheKey);
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.True, "El movimiento no debe estar en la caché de redis");
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
    }
    
    [Test]
    [Order(3)]
    public async Task GetByGuidAsync_CacheMemoria()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };
        
        var movimiento = new Movimiento
        {
            Guid = "movimiento-guid",
            ClienteGuid = "cliente-guid",
            Transferencia = transferencia
        };
        
        await _movimientoCollection.InsertOneAsync(movimiento);

        var cacheKey = "Movimientos:" + movimiento.Guid;
        _memoryCache.Set(cacheKey, movimiento);
        
        var result = await _movimientoService.GetByGuidAsync(movimiento.Guid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True, "El movimiento debe estar en la caché en memoria");
        var redisCacheValue = await _redisDatabase.Object.StringGetAsync(cacheKey);
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.True, "El movimiento no debe estar en la caché de redis");
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
    }
    
    [Test]
    [Order(4)]
    public async Task GetByGuidAsync_CacheRedis()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };

        var movimiento = new Movimiento
        {
            Guid = "movimiento-guid",
            ClienteGuid = "cliente-guid",
            Transferencia = transferencia
        };

        var redisValue = JsonSerializer.Serialize(movimiento);
        var cacheKey = "Movimientos:" + movimiento.Guid;
        _redisDatabase.Setup(db => db.StringGetAsync(cacheKey, It.IsAny<CommandFlags>())).ReturnsAsync(redisValue);

        _memoryCache.Remove(cacheKey);

        var result = await _movimientoService.GetByGuidAsync(movimiento.Guid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(_memoryCache.TryGetValue(cacheKey, out _), Is.True, "El movimiento debe estar en la caché en memoria al guardarse tras encontrarlo en redis");
        
        var redisCacheValue = await _redisDatabase.Object.StringGetAsync(cacheKey);
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.False, "El movimiento debe estar en la caché de Redis");
    }
    
    [Test]
    [Order(5)]
    public async Task GetByClienteGuidAsync()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };

        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = "nombre-comercio",
            Importe = 200,
            NumeroTarjeta = "4532111122223333"
        };
        
        var movimiento1 = new Movimiento
        {
            Guid = "movimiento1-guid",
            ClienteGuid = "cliente-1",
            Transferencia = transferencia
        };

        var movimiento2 = new Movimiento
        {
            Guid = "movimiento2-guid",
            ClienteGuid = "cliente-1",
            PagoConTarjeta = pagoConTarjeta
        };

        var clienteGuid = "cliente-1";

        await _movimientoCollection.InsertManyAsync(new List<Movimiento> { movimiento1, movimiento2 });
        
        var result = await _movimientoService.GetByClienteGuidAsync(clienteGuid);
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Guid, Is.EqualTo("movimiento1-guid"));
        Assert.That(result.First().ClienteGuid, Is.EqualTo("cliente-1"));
        Assert.That(result.First().Transferencia.Importe, Is.EqualTo(transferencia.Importe));
        Assert.That(result.Last().Guid, Is.EqualTo("movimiento2-guid"));
        Assert.That(result.Last().ClienteGuid, Is.EqualTo("cliente-1"));
        Assert.That(result.Last().PagoConTarjeta.NumeroTarjeta, Is.EqualTo(pagoConTarjeta.NumeroTarjeta));
    }
    
    [Test]
    [Order(6)]
    public async Task GetMyMovimientos()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };

        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = "nombre-comercio",
            Importe = 200,
            NumeroTarjeta = "4532111122223333"
        };
        
        var movimiento1 = new Movimiento
        {
            Guid = "movimiento1-guid",
            ClienteGuid = "cliente-1",
            Transferencia = transferencia
        };

        var movimiento2 = new Movimiento
        {
            Guid = "movimiento2-guid",
            ClienteGuid = "cliente-1",
            PagoConTarjeta = pagoConTarjeta
        };

        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-1",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        await _movimientoCollection.InsertManyAsync(new List<Movimiento> { movimiento1, movimiento2 });
        
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var result = await _movimientoService.GetMyMovimientos(userAuth);
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Guid, Is.EqualTo("movimiento1-guid"));
        Assert.That(result.First().ClienteGuid, Is.EqualTo("cliente-1"));
        Assert.That(result.First().Transferencia.Importe, Is.EqualTo(transferencia.Importe));
        Assert.That(result.Last().Guid, Is.EqualTo("movimiento2-guid"));
        Assert.That(result.Last().ClienteGuid, Is.EqualTo("cliente-1"));
        Assert.That(result.Last().PagoConTarjeta.NumeroTarjeta, Is.EqualTo(pagoConTarjeta.NumeroTarjeta));
    }

    [Test]
    [Order(7)]
    public async Task CreateAsync()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = "cliente-origen",
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100,
            Revocada = false
        };

        var movimientoRequest = new MovimientoRequest
        {
            ClienteGuid = "cliente-1",
            Transferencia = transferencia
        };
        
        var movimiento = new Movimiento
        {
            Guid = "movimiento-guid",
            ClienteGuid = "cliente-1",
            Transferencia = transferencia
        };

        var cacheKey = "Movimientos:" + movimiento.Guid;
        _memoryCache.Set(cacheKey, movimiento);
        
        var redisValue = JsonSerializer.Serialize(movimiento);
        _redisDatabase.Setup(db => db.StringGetAsync(cacheKey, It.IsAny<CommandFlags>())).ReturnsAsync(redisValue);
        
        await _movimientoService.CreateAsync(movimientoRequest);
        
        Assert.That(_memoryCache.TryGetValue(cacheKey, out var cachedMov), Is.True, "El movimiento debe estar en la memoria caché");
        var redisCacheValue = await _redisDatabase.Object.StringGetAsync(cacheKey);
        Assert.That(redisCacheValue.IsNullOrEmpty, Is.False, "El movimiento debe estar en la caché de Redis");
    }
    
    [Test]
    [Order(7)]
    public async Task CreateIngresoNominaAsync()
    {
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "nombre-empresa",
            CifEmpresa = "cif-empresa",
            IbanEmpresa = "iban-empresa",
            IbanCliente = "iban-cliente",
            Importe = 100
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban-cliente",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _cuentaService.Setup(service => service.GetByIbanAsync(ingresoNominaRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var result = await _movimientoService.CreateIngresoNominaAsync(userAuth, ingresoNominaRequest);
        
        Assert.That(result.NombreEmpresa, Is.EqualTo("nombre-empresa"));
        Assert.That(result.CifEmpresa, Is.EqualTo("cif-empresa"));
        Assert.That(result.IbanEmpresa, Is.EqualTo("iban-empresa"));
        Assert.That(result.IbanCliente, Is.EqualTo("iban-cliente"));
        Assert.That(result.Importe, Is.EqualTo(100));
    }

    [Test]
    [Order(8)]
    public void CreateIngresoNominaAsync_CuentaNotFound()
    {
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "nombre-empresa",
            CifEmpresa = "cif-empresa",
            IbanEmpresa = "iban-empresa",
            IbanCliente = "iban-cliente",
            Importe = 100
        };

        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        _cuentaService.Setup(service => service.GetByIbanAsync(ingresoNominaRequest.IbanCliente)).ReturnsAsync((CuentaResponse)null!);

        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => await _movimientoService.CreateIngresoNominaAsync(userAuth, ingresoNominaRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta con iban: {ingresoNominaRequest.IbanCliente}"));
    }
    
    [Test]
    [Order(9)]
    public void CreateIngresoNominaAsync_CuentaNoPertenecienteAlUsuario()
    {
        var ingresoNominaRequest = new IngresoNominaRequest
        {
            NombreEmpresa = "nombre-empresa",
            CifEmpresa = "cif-empresa",
            IbanEmpresa = "iban-empresa",
            IbanCliente = "iban-cliente",
            Importe = 100
        };

        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban-cliente",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "",
            ProductoGuid = "producto-guid"
        };

        _cuentaService.Setup(service => service.GetByIbanAsync(ingresoNominaRequest.IbanCliente)).ReturnsAsync(cuentaResponse);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);

        var ex = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>(async () => await _movimientoService.CreateIngresoNominaAsync(userAuth, ingresoNominaRequest));

        Assert.That(ex.Message, Is.EqualTo($"La cuenta con guid {cuentaResponse.Guid} no pertenece al cliente con guid {clienteResponse.Guid}"));
    }
    
    [Test]
    [Order(10)]
    public async Task CreatePagoConTarjeta()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "nombre-comercio",
            Importe = 100,
            NumeroTarjeta = "1111222233334444",
        };

        var tarjetaResponse = new TarjetaResponse
        {
            Guid = "tarjeta-guid",
            Numero = "1111222233334444",
            FechaVencimiento = "01/30",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 100,
            LimiteSemanal = 500,
            LimiteMensual = 1500
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban-cliente",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _tarjetaService.Setup(service => service.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta)).ReturnsAsync(tarjetaResponse);
        _cuentaService.Setup(service => service.GetByTarjetaGuidAsync(tarjetaResponse.Guid)).ReturnsAsync(cuentaResponse);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var result = await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest);
        
        Assert.That(result.NombreComercio, Is.EqualTo("nombre-comercio"));
        Assert.That(result.Importe, Is.EqualTo(100));
        Assert.That(result.NumeroTarjeta, Is.EqualTo("1111222233334444"));
    }
    
    [Test]
    [Order(11)]
    public void CreatePagoConTarjeta_TarjetaNotFound()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "nombre-comercio",
            Importe = 100,
            NumeroTarjeta = "1111222233334444",
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };
        
        _tarjetaService.Setup(service => service.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta)).ReturnsAsync((TarjetaResponse)null!);
        
        var ex = Assert.ThrowsAsync<TarjetaNotFoundException>(async () => await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la tarjeta con número: {pagoConTarjetaRequest.NumeroTarjeta}"));
    }
    
    [Test]
    [Order(12)]
    public void CreatePagoConTarjeta_CuentaNotFound()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "nombre-comercio",
            Importe = 100,
            NumeroTarjeta = "1111222233334444",
        };

        var tarjetaResponse = new TarjetaResponse
        {
            Guid = "tarjeta-guid",
            Numero = "1111222233334444",
            FechaVencimiento = "01/30",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 100,
            LimiteSemanal = 500,
            LimiteMensual = 1500
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };
        
        _tarjetaService.Setup(service => service.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta)).ReturnsAsync(tarjetaResponse);
        _cuentaService.Setup(service => service.GetByTarjetaGuidAsync(tarjetaResponse.Guid)).ReturnsAsync((CuentaResponse)null!);
        
        var ex = Assert.ThrowsAsync<CuentaNotFoundException>(async () => await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest));

        Assert.That(ex.Message, Is.EqualTo($"No se ha encontrado la cuenta asociada a la tarjeta con guid: {tarjetaResponse.Guid}"));
    }
    
    [Test]
    [Order(13)]
    public void CreatePagoConTarjeta_CuentaNoPertenecienteAlUsuario()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "nombre-comercio",
            Importe = 100,
            NumeroTarjeta = "1111222233334444",
        };

        var tarjetaResponse = new TarjetaResponse
        {
            Guid = "tarjeta-guid",
            Numero = "1111222233334444",
            FechaVencimiento = "01/30",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 100,
            LimiteSemanal = 500,
            LimiteMensual = 1500
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban-cliente",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "",
            ProductoGuid = "producto-guid"
        };
        
        _tarjetaService.Setup(service => service.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta)).ReturnsAsync(tarjetaResponse);
        _cuentaService.Setup(service => service.GetByTarjetaGuidAsync(tarjetaResponse.Guid)).ReturnsAsync(cuentaResponse);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var ex = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>(async () => await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest));

        Assert.That(ex.Message, Is.EqualTo($"La tarjeta con guid {tarjetaResponse.Guid} perteneciente a la cuenta con guid {cuentaResponse.Guid} no pertenece al cliente con guid {clienteResponse.Guid}"));
    }
    
    [Test]
    [Order(14)]
    public void CreatePagoConTarjeta_SaldoCuentaInsuficiente()
    {
        var pagoConTarjetaRequest = new PagoConTarjetaRequest
        {
            NombreComercio = "nombre-comercio",
            Importe = 1000,
            NumeroTarjeta = "1111222233334444",
        };

        var tarjetaResponse = new TarjetaResponse
        {
            Guid = "tarjeta-guid",
            Numero = "1111222233334444",
            FechaVencimiento = "01/30",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 100,
            LimiteSemanal = 500,
            LimiteMensual = 1500
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponse = new CuentaResponse
        {
            Guid = "cuenta-guid",
            Iban = "iban-cliente",
            Saldo = 500,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        _tarjetaService.Setup(service => service.GetByNumeroTarjetaAsync(pagoConTarjetaRequest.NumeroTarjeta)).ReturnsAsync(tarjetaResponse);
        _cuentaService.Setup(service => service.GetByTarjetaGuidAsync(tarjetaResponse.Guid)).ReturnsAsync(cuentaResponse);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        
        var ex = Assert.ThrowsAsync<SaldoCuentaInsuficientException>(async () => await _movimientoService.CreatePagoConTarjetaAsync(userAuth, pagoConTarjetaRequest));

        Assert.That(ex.Message, Is.EqualTo($"Saldo insuficiente en la cuenta con guid: {cuentaResponse.Guid} respecto al importe de {pagoConTarjetaRequest.Importe} €"));
    }
    
    /*[Test]
    [Order(13)]
    public async Task CreateTransferenciaAsync()
    {
        var transferenciaRequest = new TransferenciaRequest
        {
            IbanOrigen = "iban-origen",
            NombreBeneficiario = "nombre-beneficiario",
            IbanDestino = "iban-destino",
            Importe = 100
        };
        
        var userAuth = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "username",
            Password = "password",
            Role = Role.Cliente
        };

        var clienteResponse = new ClienteResponse
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            UserResponse = new UserResponse
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };

        var cuentaResponseOrigen = new CuentaResponse
        {
            Guid = "cuenta-origen-guid",
            Iban = "iban-origen",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };
        
        var cuentaResponseDestino = new CuentaResponse
        {
            Guid = "cuenta-destino-guid",
            Iban = "iban-destino",
            Saldo = 5000,
            TarjetaGuid = "tarjeta-guid",
            ClienteGuid = "cliente-guid",
            ProductoGuid = "producto-guid"
        };

        var clienteEntity = new ClienteEntity()
        {
            Guid = "cliente-guid",
            Nombre = "Cliente",
            Apellidos = "Apellido",
            Email = "cliente@correo.com",
            Telefono = "123456789",
            User = new UserEntity
            {
                Guid = "user-guid",
                Username = "username",
                Password = "password"
            }
        };
        
        _cuentaService.Setup(service => service.GetByIbanAsync(transferenciaRequest.IbanOrigen)).ReturnsAsync(cuentaResponseOrigen);
        _clienteService.Setup(service => service.GetMeAsync(userAuth)).ReturnsAsync(clienteResponse);
        _cuentaService.Setup(service => service.GetByIbanAsync(transferenciaRequest.IbanDestino)).ReturnsAsync(cuentaResponseDestino);
        
        var result = await _movimientoService.CreateTransferenciaAsync(userAuth, transferenciaRequest);
        
        Assert.That(result.IbanOrigen, Is.EqualTo("iban-origen"));
        Assert.That(result.NombreBeneficiario, Is.EqualTo("nombre-beneficiario"));
        Assert.That(result.IbanDestino, Is.EqualTo("iban-destino"));
        Assert.That(result.Importe, Is.EqualTo(100));
    }*/
}