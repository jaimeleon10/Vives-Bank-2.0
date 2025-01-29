﻿using System.Text.Json;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.Utils.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Role = Banco_VivesBank.User.Models.Role;

namespace Test.Producto.Tarjeta.Services;

[TestFixture]
public class TarjetaServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private TarjetaService _tarjetaService;
    private IMemoryCache _memoryCache;
    private Mock<IConnectionMultiplexer> _redisConnectionMock;
    private Mock<IMemoryCache> _memoryCacheMock;
    private Mock<ICacheEntry> _cacheEntryMock;
    private Mock<IDatabase> _mockDatabase;

    [OneTimeSetUp]
    public async Task Setup()
    {
        // Configuración del contenedor PostgreSQL
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();

        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;
        
        
        
        _dbContext = new GeneralDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
       
        // Crear mocks
        _redisConnectionMock = new Mock<IConnectionMultiplexer>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _mockDatabase = new Mock<IDatabase>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _redisConnectionMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_mockDatabase.Object);
        
        _tarjetaService = new TarjetaService(
            _dbContext,
            NullLogger<TarjetaService>.Instance,
            NullLogger<CardLimitValidators>.Instance,
            _redisConnectionMock.Object, // Mock de IConnectionMultiplexer
            _memoryCache
        );
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
        if (_memoryCache != null)
        {
            _memoryCache.Dispose();
        }
    }

    [Test]
    public async Task GetByGuid()
    {
        var guid = "Guid-Prueba";
        var tarjeta1 = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta1);
        var serializedUser = JsonSerializer.Serialize(tarjeta1);
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));
        await _dbContext.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetByGuidAsync(guid);
        Assert.That(tarjeta.Guid, Is.EqualTo(guid));
    }

    [Test]
    public async Task GetByGuid_NotFound()
    {
        var guid = "Guid-Prueba";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.GetByGuidAsync("non-existing-guid");
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task GetByNumero()
    {
        var guid = "Guid-Prueba";
        var tarjeta1 = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta1);
        await _dbContext.SaveChangesAsync();

        var tarjeta = await _tarjetaService.GetByNumeroTarjetaAsync("1234567890123456");
        Assert.That(tarjeta1.Guid, Is.EqualTo(guid));
    }
    
    [Test]
    public async Task GetByNumeroNotFound()
    {
        var numero = "Guid-Prueba";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.GetByNumeroTarjetaAsync(numero);
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    

    [Test]
    public async Task Create()
    {
        
        var nuevaCuenta = new CuentaEntity
        {
            Guid = "CuentaGuidTest",
            Iban = "ES7620770024003102575766", // IBAN de ejemplo
            Saldo = 0, // Saldo inicial
            TarjetaId = null, // Sin tarjeta asociada por ahora
            ClienteId = 1, // Id de un cliente existente
            ProductoId = 2, // Id de un producto existente
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _dbContext.AddAsync(nuevaCuenta);
        await _dbContext.SaveChangesAsync();

        
        var tarjetaRequest = new TarjetaRequest
        {
            CuentaGuid = "CuentaGuidTest",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };
        
        
        
        var tarjeta = await _tarjetaService.CreateAsync(tarjetaRequest);
        var serializedUser = JsonSerializer.Serialize(tarjeta);
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));

        Assert.That(tarjeta.Pin, Is.EqualTo("1234"));
        Assert.That(tarjeta.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async Task Update()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };

        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var serializedUser = JsonSerializer.Serialize(tarjeta);
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));
        var tarjetaActualizada = await _tarjetaService.UpdateAsync(guid, tarjetaRequest);

        Assert.That(tarjetaActualizada.Pin, Is.EqualTo("1234"));
        Assert.That(tarjetaActualizada.LimiteDiario, Is.EqualTo(1000));
    }

    [Test]
    public async Task Update_NotFound()
    {
        var tarjetaRequest = new TarjetaRequestUpdate()
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _tarjetaService.UpdateAsync("non-existing-guid", tarjetaRequest);
        Assert.That(tarjetaNoExiste, Is.Null);
    }
    
    [Test]
    public async Task UpdateInvalidLimitsDia()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(guid, tarjetaRequest));
        
        
    }
    
    [Test]
    public async Task UpdateInvalidLimitsSem()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(guid, tarjetaRequest));

    }

    [Test]
    public async Task UpdateInvalidLimitsMes()
    {
        var tarjetaRequest = new TarjetaRequestUpdate
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 5000,
            LimiteMensual = 3000
        };
        
        var guid = "NuevoGuid";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";

        // Test con tarjeta que no existe
        Assert.ThrowsAsync<TarjetaNotFoundException>(() => _tarjetaService.UpdateAsync(guid, tarjetaRequest));

    }
    
    [Test]
    public async Task Delete()
    {
        
        var nuevaCuenta = new CuentaEntity
        {
            Guid = "CuentaGuidTest",
            Iban = "ES7620770024003102575766", // IBAN de ejemplo
            Saldo = 0, // Saldo inicial
            TarjetaId = null, // Sin tarjeta asociada por ahora
            ClienteId = 1, // Id de un cliente existente
            ProductoId = 2, // Id de un producto existente
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _dbContext.AddAsync(nuevaCuenta);
        await _dbContext.SaveChangesAsync();
        
        var guid = "guid-test";
        var tarjeta = new TarjetaEntity
        {
            Guid = guid,
            Numero = "1234567890123456",
            Cvv = "123",
            FechaVencimiento = "01/23",
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 3000,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
     
        var serializedUser = JsonSerializer.Serialize(tarjeta);
        _memoryCache.Set("tarjetaTest", serializedUser, TimeSpan.FromMinutes(30));
         _dbContext.Tarjetas.Add(tarjeta);
        await _dbContext.SaveChangesAsync();

        nuevaCuenta.Tarjeta = tarjeta;
        await _dbContext.Cuentas.FirstOrDefaultAsync(c => c.Tarjeta!.Guid == guid);
        await _tarjetaService.DeleteAsync(tarjeta.Guid);

        var tarjetaBorrada = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == tarjeta.Guid);
        Assert.That(tarjetaBorrada.IsDeleted,  Is.True);
    }

    [Test]
    public async Task Delete_NotFound()
    {
        var nonExistingTarjetaGuid = "Non-existing-tarjeta-guid";
        await _tarjetaService.DeleteAsync(nonExistingTarjetaGuid);

        // Test con tarjeta que no existe
        var tarjetaNoExiste = await _dbContext.Tarjetas.FirstOrDefaultAsync(t => t.Guid == nonExistingTarjetaGuid);
        Assert.That(tarjetaNoExiste, Is.Null);

    }

}