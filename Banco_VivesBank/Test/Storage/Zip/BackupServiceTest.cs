using Banco_VivesBank.Database;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using Testcontainers.PostgreSql;

[TestFixture]
public class BackupServiceTests
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IStorageJson> _storageJsonMock;
    private Mock<IOptions<MovimientosMongoConfig>> _movimientosDatabaseSettingsMock;
    private GeneralDbContext _realDbContext;
    private GeneralDbContext _testDbContext;
    private BackupService _backupService;
    private PostgreSqlContainer _postgreSqlContainer;
    private Mock<IMongoCollection<Movimiento>> _movimientosCollectionMock;
    private Mock<IMongoCollection<Domiciliacion>> _domiciliacionesCollectionMock;
    private Mock<IMongoDatabase> _mongoDatabaseMock;
    private Mock<IMongoClient> _mongoClientMock;

    [OneTimeSetUp]
    public async Task Setup()
    {
        try
        {
            // Initialize all mocks
            _loggerMock = new Mock<ILogger<BackupService>>();
            _storageJsonMock = new Mock<IStorageJson>();
            _movimientosDatabaseSettingsMock = new Mock<IOptions<MovimientosMongoConfig>>();
            _movimientosCollectionMock = new Mock<IMongoCollection<Movimiento>>();
            _domiciliacionesCollectionMock = new Mock<IMongoCollection<Domiciliacion>>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _mongoClientMock = new Mock<IMongoClient>();

            // Configure MongoDB mocks
            _mongoDatabaseMock
                .Setup(db => db.GetCollection<Movimiento>(It.IsAny<string>(), null))
                .Returns(_movimientosCollectionMock.Object);
            _mongoDatabaseMock
                .Setup(db => db.GetCollection<Domiciliacion>(It.IsAny<string>(), null))
                .Returns(_domiciliacionesCollectionMock.Object);
            _mongoClientMock
                .Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mongoDatabaseMock.Object);

            // Setup MongoDB settings
            var mongoConfig = new MovimientosMongoConfig
            {
                ConnectionString = "mongodb://admin:password@localhost:27017",
                DatabaseName = "MovimientosDB",
                MovimientosCollectionName = "Movimientos",
                DomiciliacionesCollectionName = "Domiciliaciones"
            };
            _movimientosDatabaseSettingsMock.Setup(x => x.Value).Returns(mongoConfig);

            // Setup PostgreSQL container
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithPortBinding(5432, true)
                .Build();

            await _postgreSqlContainer.StartAsync();

            // Setup DbContexts
            var realDbOptions = new DbContextOptionsBuilder<GeneralDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=VivesBankDB;Username=admin;Password=password")
                .Options;

            var testDbOptions = new DbContextOptionsBuilder<GeneralDbContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                .Options;

            _realDbContext = new GeneralDbContext(realDbOptions);
            _testDbContext = new GeneralDbContext(testDbOptions);

            await _testDbContext.Database.EnsureDeletedAsync();
            await _testDbContext.Database.MigrateAsync();

            // Setup StorageJson mock
            _storageJsonMock
                .Setup(s => s.ExportJson(It.IsAny<FileInfo>(), It.IsAny<List<object>>()))
                .Callback<FileInfo, List<object>>((file, data) =>
                {
                    Directory.CreateDirectory(file.DirectoryName);
                    var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(file.FullName, json);
                });

            // Initialize BackupService
            _backupService = new BackupService(
                _loggerMock.Object,
                _realDbContext,
                _storageJsonMock.Object,
                _movimientosDatabaseSettingsMock.Object
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Setup failed: {ex}");
            throw;
        }
    }

    [Test]
    public async Task ExportToZip_ValidData_ExportsToZipFile()
    {
        // Test implementation remains the same
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");

        try
        {
            var movimientos = new List<Movimiento>
            {
                new Movimiento
                {
                    Id = "mov1",
                    ClienteGuid = "222",
                    IngresoNomina = new IngresoNomina
                    {
                        Importe = 2000,
                        CifEmpresa = "B31980295",
                        IbanCliente = "ES9820958396559949373938",
                        IbanEmpresa = "ES9820958394559949373938",
                        NombreEmpresa = "B31980295"
                    },
                    CreatedAt = DateTime.Now
                }
            };

            var domiciliaciones = new List<Domiciliacion>
            {
                new Domiciliacion
                {
                    Id = "dom1",
                    ClienteGuid = "222",
                    Guid = "dom1",
                    Activa = true,
                    IbanEmpresa = "ES9820958394545949373938",
                    IbanCliente = "ES9820958396559949373938",
                    Acreedor = "Banco de España",
                    Importe = 12999,
                    FechaInicio = DateTime.UtcNow
                }
            };

            _movimientosCollectionMock
                .Setup(c => c.InsertManyAsync(
                    It.IsAny<IEnumerable<Movimiento>>(),
                    It.IsAny<InsertManyOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _domiciliacionesCollectionMock
                .Setup(c => c.InsertManyAsync(
                    It.IsAny<IEnumerable<Domiciliacion>>(),
                    It.IsAny<InsertManyOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _movimientosCollectionMock.Object.InsertManyAsync(movimientos);
            await _domiciliacionesCollectionMock.Object.InsertManyAsync(domiciliaciones);

            await _backupService.ExportToZip(sourceDirectory, zipFilePath);

            Assert.That(File.Exists(zipFilePath), Is.True);
        }
        finally
        {
            Cleanup(zipFilePath, sourceDirectory);
        }
    }

    private string CreateTempDirectory()
    {
        string testTempPath = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        Directory.CreateDirectory(testTempPath);
        string tempDir = Path.Combine(testTempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private void Cleanup(params string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up {path}: {ex}");
            }
        }
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        try
        {
            if (_testDbContext != null) await _testDbContext.DisposeAsync();
            if (_realDbContext != null) await _realDbContext.DisposeAsync();
            if (_postgreSqlContainer != null)
            {
                await _postgreSqlContainer.StopAsync();
                await _postgreSqlContainer.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Teardown failed: {ex}");
            throw;
        }
    }
}

/*
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
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();
        
        _mockMovimientosCollection = new Mock<IMongoCollection<Movimiento>>();
        _mockDomiciliacionesCollection = new Mock<IMongoCollection<Domiciliacion>>();
        _mockMongoDatabase = new Mock<IMongoDatabase>();
        _mockMongoClient = new Mock<IMongoClient>();

        _mockMongoDatabase
            .Setup(db => db.GetCollection<Movimiento>(It.IsAny<string>(), null))
            .Returns(_mockMovimientosCollection.Object);
        _mockMongoDatabase
            .Setup(db => db.GetCollection<Domiciliacion>(It.IsAny<string>(), null))
            .Returns(_mockDomiciliacionesCollection.Object);

        _mockMongoClient
            .Setup(client => client.GetDatabase(It.IsAny<string>(), null))
            .Returns(_mockMongoDatabase.Object);

        _loggerMock = new Mock<ILogger<BackupService>>();
        _storageJsonMock = new Mock<IStorageJson>();

        _storageJsonMock
            .Setup(s => s.ExportJson(It.IsAny<FileInfo>(), It.IsAny<List<object>>()))
            .Callback<FileInfo, List<object>>((file, data) =>
            {
                Directory.CreateDirectory(file.DirectoryName);
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(file.FullName, json);
            });
        
        var mongoConfig = Options.Create(new MovimientosMongoConfig
        {
            ConnectionString = "mongodb://admin:password@localhost:27017",
            DatabaseName = "MovimientosDB",
            MovimientosCollectionName = "Movimientos",
            DomiciliacionesCollectionName = "Domiciliaciones"
        });

        _backupService = new BackupService(
            _loggerMock.Object,
            _dbContext,
            _storageJsonMock.Object,
            mongoConfig
        );
    }
    
    [Test]
    public void ExportToZipDirectoryNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.ExportToZip(null, "test.zip"));
    }

    [Test]
    public void ExportToZipFilePathNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.ExportToZip("source", null));
    }

    [Test]
    public async Task ExportToZip()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        Assert.That(File.Exists(zipFilePath), Is.True);
        Cleanup(zipFilePath, sourceDirectory);
    }

    [Test]
    public async Task ExportToZipJsonCorrectosZip()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var expectedFiles = new[] { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" };

        // Act
        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Assert
        using var zip = ZipFile.OpenRead(zipFilePath);
        foreach (var file in expectedFiles)
        {
            Assert.That(zip.Entries.Any(e => e.Name == file), Is.True, $"Falta el archivo {file} en el ZIP");
        }
        Cleanup(zipFilePath, sourceDirectory);
    }

    [Test]
    public async Task ExportToZipSobreEscribeZip()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        File.WriteAllText(zipFilePath, "dummy content");

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        using var zip = ZipFile.OpenRead(zipFilePath);
        Assert.That(zip.Entries.Count, Is.GreaterThan(0));
        Cleanup(zipFilePath, sourceDirectory);
    }

    [Test]
    public async Task ExportToZipCopiaAvatares()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var avatarDir = Path.Combine(sourceDirectory, "avatares");
        Directory.CreateDirectory(avatarDir);
        var avatarPath = Path.Combine(avatarDir, "test.png");
        File.WriteAllText(avatarPath, "dummy");

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        using var zip = ZipFile.OpenRead(zipFilePath);
        Assert.That(zip.Entries.Any(e => e.FullName.StartsWith("avatares/")), Is.True);
        Cleanup(zipFilePath, sourceDirectory);
    }
    
    [Test]
    public void ExportarDatosAJson()
    {
        // Arrange
        string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "temp_test");
        Directory.CreateDirectory(tempDir);

        var usuarios = new List<UserEntity>
        {
             new UserEntity { Id = 1, Username = "Usuario1" , IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Guid = "user_1_guid", Password = "sjjwjf3232", Role = Role.Cliente},
             new UserEntity { Id = 2, Username = "Usuario2" , IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Guid = "user_2_guid", Password = "sjjwjf3kfk32", Role = Role.User}
        };
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Id = 1,
            Nombre = "Cliente1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Guid = "client_1_guid",
            Apellidos = "apellido",
            Direccion = new Direccion
            {
                Calle = "Calle1",
                Numero = "1",
                Piso = "1",
                CodigoPostal = "12345",
            },
            Telefono = "123456789",
            Email = "usuario1@example.com",
        };
        var clientes = new List<Banco_VivesBank.Cliente.Models.Cliente> { cliente };
        var productos = new List<ProductoEntity> { 
            new ProductoEntity {
                Id = 1, 
                Nombre = "Producto1",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Guid = "product_1_guid",
                Descripcion = "ff",
                TipoProducto = "Cuenta"
            } 
        };
        var cuentas = new List<Cuenta>
        {
            new Cuenta { 
                Id = 1, 
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Guid = "account_1_guid",
                Saldo = 1000,
            }
        };
        var tarjetas = new List<Tarjeta> { new Tarjeta { Id = 1, Numero = "98765" } };
        var movimientos = new List<Movimiento> { new Movimiento { Id = "1l", Guid = "fwhjd", ClienteGuid = "jbvdkbv"} };
        var domiciliaciones = new List<Domiciliacion> { new Domiciliacion { Id = "1l", Importe = 1000, ClienteGuid = "jgkj", 
                                IbanCliente = "ES5300759569216975477421", Acreedor = "jfdjd", IbanEmpresa = "ES8914656556815361769382"} };

        // Act
        _backupService.ExportarDatosAJson(tempDir, usuarios, clientes, productos, cuentas, tarjetas, movimientos, domiciliaciones);

        // Assert
        Assert.That(File.Exists(Path.Combine(tempDir, "usuarios.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "clientes.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "productos.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "cuentas.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "tarjetas.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "movimientos.json")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDir, "domiciliaciones.json")), Is.True);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public async Task ImportFromZipFileNoExiste()
    {
        var zipFilePath = Path.Combine(Path.GetTempPath(), "nonexistent.zip");
        var destinationDirectory = CreateTempDirectory();

        Assert.ThrowsAsync<ImportFromZipException>(() => 
            _backupService.ImportFromZip(zipFilePath, destinationDirectory));
    }

    [Test]
    public async Task ImportFromZipZipValido()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();

        await _dbContext.Usuarios.AddAsync(new UserEntity { Guid = Guid.NewGuid().ToString() });
        await _dbContext.SaveChangesAsync();

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        Assert.That(await _dbContext.Usuarios.AnyAsync(), Is.True);
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory);
    }

    [Test]
    public async Task ImportFromZipEvitaEntidadesExistentes()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();

        var originalUser = new UserEntity { Guid = "test-guid" };
        await _dbContext.Usuarios.AddAsync(originalUser);
        await _dbContext.SaveChangesAsync();

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);
        _dbContext.Usuarios.Remove(originalUser);
        await _dbContext.SaveChangesAsync();

        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        Assert.That(await _dbContext.Usuarios.CountAsync(), Is.EqualTo(1));
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory);
    }

    [Test]
    public async Task ImportFromZipCopiaAvatarDirectory()
    {
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();
        var avatarPath = Path.Combine("data", "avatares", "test.png");
        Directory.CreateDirectory(Path.GetDirectoryName(avatarPath));
        File.WriteAllText(avatarPath, "dummy");

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        Assert.That(File.Exists(avatarPath), Is.True);
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory, avatarPath);
    }

    [Test]
    public async Task SaveEntidadesSiNoExistenAsync()
    {
        var existingUser = new UserEntity { Guid = "existing-guid" };
        await _dbContext.Usuarios.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var newUser = new UserEntity { Guid = "new-guid" };
        var users = new List<UserEntity> { existingUser, newUser };

        await _backupService.SaveEntidadesSiNoExistenAsync(
            users,
            _dbContext.Usuarios,
            u => u.Id.ToString(),
            u => u.Guid
        );

        Assert.That(await _dbContext.Usuarios.CountAsync(), Is.EqualTo(2));
    }

    private string CreateTempDirectory()
    {
        string testTempPath = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        Directory.CreateDirectory(testTempPath);
        string tempDir = Path.Combine(testTempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
    
    private void Cleanup(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path)) File.Delete(path);
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await _dbContext.DisposeAsync();
        await _postgreSqlContainer.StopAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}*/