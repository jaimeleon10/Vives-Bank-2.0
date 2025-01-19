using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Renci.SshNet.Common;
using Testcontainers.PostgreSql;
using BigInteger = System.Numerics.BigInteger;

namespace Banco_VivesBank.Test.Producto.Cuenta.Services;

[TestFixture]
[TestOf(typeof(CuentaService))]
public class CuentaServiceTests
{
    private PostgreSqlContainer _postgreSqlContainer;
    private GeneralDbContext _dbContext;
    private CuentaService _cuentaService;

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

        var options = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new GeneralDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        var baseService = new Mock<IBaseService>().Object;
        _cuentaService = new CuentaService(_dbContext, NullLogger<CuentaService>.Instance, new Mock<IBaseService>().Object);
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
    }

    [Test]
    public async Task GetAll()
    {
        var cuenta1 = new CuentaEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Iban = "ES1234567890123456789012",
            Saldo = 1000,
            ClienteId = 1,
            ProductoId = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var cuenta2 = new CuentaEntity
        {
            Guid = Guid.NewGuid().ToString(),
            Iban = "ES9876543210987654321098",
            Saldo = 2000,
            ClienteId = 2,
            ProductoId = 2,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Cuentas.AddRange(cuenta1, cuenta2);
        await _dbContext.SaveChangesAsync();

        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10,
            SortBy = "Saldo",
            Direction = "ASC"
        };

        var result = await _cuentaService.GetAll(1500, 500, "Ahorro", pageRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content.First().Saldo, Is.EqualTo(1000));
    }

    [Test]
    public async Task GetAll_NoExisteTipoCuenta()
    {
        var pageRequest = new PageRequest
        {
            PageNumber = 0,
            PageSize = 10
        };
        
        var result = await _cuentaService.GetAll(5000, 4000, "NoExistente", pageRequest);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content.Count, Is.EqualTo(0));
        Assert.That(result.Empty, Is.True);
    }

    [Test]
    public async Task GetByClientGuid()
    {
        var clientGuid = "cliente123";
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente { Guid = clientGuid, Nombre = "Cliente Test", Apellidos = "apellidos", Email = "email", Telefono = "telefono", Dni = "dni", Id = 1 };
        var producto = new BaseModel { Id = 1, Nombre = "Cuenta Ahorro" };

        var cuenta1 = new CuentaEntity
        {
            Guid = "cuenta1",
            Iban = "IBAN001",
            Saldo = 1000,
            Cliente = cliente,
            Producto = producto
        };

        var cuenta2 = new CuentaEntity
        {
            Guid = "cuenta2",
            Iban = "IBAN002",
            Saldo = 2000,
            Cliente = cliente,
            Producto = producto
        };

        _dbContext.Clientes.Add(ClienteMapper.ToEntityFromModel(cliente));
        _dbContext.ProductoBase.Add(BaseMapper.ToEntityFromModel(producto));
        _dbContext.Cuentas.AddRange(cuenta1, cuenta2);
        await _dbContext.SaveChangesAsync();
        
        var result = await _cuentaService.getByClientGuid(clientGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var firstCuenta = result.First();
        Assert.That(firstCuenta.Guid, Is.EqualTo("cuenta1"));
        Assert.That(firstCuenta.Iban, Is.EqualTo("IBAN001"));
        Assert.That(firstCuenta.Saldo, Is.EqualTo(1000));
        Assert.That(firstCuenta.ClienteId, Is.EqualTo(1));
        Assert.That(firstCuenta.ProductoId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByClientGuid_Invalido()
    {
        var invalidGuid = "noexiste";
        
        var result = await _cuentaService.getByClientGuid(invalidGuid);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetByGuid()
    {
        var cuentaGuid = Guid.NewGuid().ToString();
        var cuenta = new CuentaEntity { Guid = cuentaGuid, Iban = "ES9876543210987654321098", Saldo = 300, Cliente = new Banco_VivesBank.Cliente.Models.Cliente { Guid = "client1" } };

        _dbContext.Cuentas.Add(cuenta);
        await _dbContext.SaveChangesAsync();

        var result = await _cuentaService.getByGuid(cuentaGuid);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cuentaGuid));
    }
    
    [Test]
    public async Task GetByIban()
    {
        var result = await _cuentaService.getByIban("ES1234567890123456789012");
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
        Assert.That(result.Saldo, Is.EqualTo((BigInteger)1000));
    }

    [Test]
    public async Task GetByIban_CuentaNotExits()
    {
        var ex = Assert.ThrowsAsync<CuentaNoEncontradaException>(async () =>
            await _cuentaService.getByIban("ES0000000000000000000000"));

        Assert.That(ex.Message, Is.EqualTo("Cuenta con IBAN ES0000000000000000000000 no encontrada."));
    }

    /*[Test]
    public async Task Save()
    {
        var cuentaRequest = new CuentaRequest { TipoCuenta = "Ahorro" };
        var clientGuid = "client1";
        var result = await _cuentaService.save(clientGuid, cuentaRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.TipoCuenta, Is.EqualTo("Ahorro"));
    }*/

    [Test]
        public async Task Update_SaldoInsuficienteException()
        {
            
            var ex = Assert.ThrowsAsync<SaldoInsuficienteException>(async () =>
                await _cuentaService.update("12345", "nonexistent-guid", new CuentaUpdateRequest { Dinero = "500" }));

            Assert.That(ex.Message, Is.EqualTo("La cuenta con el GUID nonexistent-guid no existe."));
        }

        [Test]
        public async Task Update_CuentaNoPertenecienteAlUsuarioException()
        {
            var ex = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>(async () =>
                await _cuentaService.update("wrong-guid", "abc123", new CuentaUpdateRequest { Dinero = "500" }));

            Assert.That(ex.Message, Is.EqualTo("Cuenta con IBAN: ES1234567890123456789012 no le pertenece"));
        }

        [Test]
        public async Task Update_SaldoInvalidoException()
        {
            var ex = Assert.ThrowsAsync<SaldoInvalidoException>(async () =>
                await _cuentaService.update("12345", "abc123", new CuentaUpdateRequest { Dinero = "invalidSaldo" }));

            Assert.That(ex.Message, Is.EqualTo("El saldo proporcionado no es válido."));
        }

        

        [Test]
        public async Task Update()
        {
            var cuentaUpdateRequest = new CuentaUpdateRequest { Dinero = "500" };
            var cuentaRequest = new CuentaRequest { TipoCuenta = "Ahorro" };

            await _cuentaService.save("abc123", cuentaRequest);
    
            var savedCuenta = await _dbContext.Cuentas.FirstOrDefaultAsync(c => c.Guid == "abc123");
    
            Assert.That(savedCuenta, Is.Not.Null, "La cuenta no fue guardada correctamente.");
            Assert.That(savedCuenta.Guid, Is.EqualTo("abc123"));
    
            var resultUpdate = await _cuentaService.update("12345", "abc123", cuentaUpdateRequest);
    
            Assert.That(resultUpdate, Is.Not.Null);
            Assert.That(resultUpdate.Saldo, Is.EqualTo(500));
            Assert.That(resultUpdate.Guid, Is.EqualTo("abc123"));
        }
        
        [Test]
        public void Update_NotFound()
        {
            var nonExistingCuentaGuid = "CuentaNoExiste";
            var updateRequest = new CuentaUpdateRequest { Dinero = "400" };

            var ex = Assert.ThrowsAsync<CuentaNoEncontradaException>(async () =>
                await _cuentaService.update("client1", nonExistingCuentaGuid, updateRequest)
            );

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo($"Cuenta with GUID {nonExistingCuentaGuid} not found"));
        }
        
        [Test]
        public async Task Delete()
        {
            var cuentaGuid = "existing-cuenta-guid"; 
            var clienteGuid = 1L;
    
            var cuenta = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta { Guid = cuentaGuid, ClienteId = clienteGuid, IsDeleted = false };
            await _dbContext.Cuentas.AddAsync(cuenta.ToCuentaEntity());
            await _dbContext.SaveChangesAsync();
    
            var result = await _cuentaService.delete(clienteGuid.ToString(), cuentaGuid);
    
            var deletedCuenta = await _dbContext.Cuentas.FindAsync(cuentaGuid);
            Assert.That(deletedCuenta, Is.Not.Null);
            Assert.That(deletedCuenta.IsDeleted, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Guid, Is.EqualTo(cuentaGuid));
        }

        [Test]
        public void Delete_NotFound()
        {
            var nonExistingCuentaGuid = "CuentaNoExiste";

            var ex = Assert.ThrowsAsync<SaldoInsuficienteException>(async () =>
                await _cuentaService.delete("client1", nonExistingCuentaGuid)
            );

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo($"La cuenta con el GUID {nonExistingCuentaGuid} no existe."));
        }

        [Test]
        public void Delete_CuentaNoPertenecienteAlUsuarioException()
        {
            var cuentaGuid = "existing-cuenta-guid"; 
            var wrongClientGuid = "wrong-client-guid"; 

            var ex = Assert.ThrowsAsync<CuentaNoPertenecienteAlUsuarioException>(async () =>
                await _cuentaService.delete(wrongClientGuid, cuentaGuid)
            );

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo($"Cuenta con IBAN: null  no le pertenece"));
        }
}