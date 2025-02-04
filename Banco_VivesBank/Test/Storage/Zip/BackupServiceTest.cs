using System.IO.Compression;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.Storage.Zip.Services;
using Banco_VivesBank.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace Test.Storage.Zip;

[TestFixture]
public class BackupServiceTests : IAsyncDisposable
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IStorageJson> _storageJsonMock;
    private Mock<IOptions<MovimientosMongoConfig>> _movimientosDatabaseSettingsMock;
    private Mock<IMongoCollection<Movimiento>> _movimientosCollectionMock;
    private GeneralDbContext _testDbContext;
    private BackupService _backupService;
    private PostgreSqlContainer _postgreSqlContainer;
    private MongoDbContainer _mongoDbContainer;
    private IMongoClient _mongoClient;
    private IMongoDatabase _mongoDatabase;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _storageJsonMock = new Mock<IStorageJson>();
        _movimientosDatabaseSettingsMock = new Mock<IOptions<MovimientosMongoConfig>>();
        _movimientosCollectionMock = new Mock<IMongoCollection<Movimiento>>();

        await InitializeContainers();
        await InitializeDatabases();
        ConfigureStorageJsonMock();

        _backupService = new BackupService(
            _loggerMock.Object,
            _testDbContext,
            _storageJsonMock.Object,
            _movimientosDatabaseSettingsMock.Object
        );
    }

    private async Task InitializeContainers()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:4.4")
            .WithCleanUp(true)
            .Build();

        await Task.WhenAll(
            _postgreSqlContainer.StartAsync(),
            _mongoDbContainer.StartAsync()
        );
    }

    private async Task InitializeDatabases()
    {
        var testDbOptions = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _testDbContext = new GeneralDbContext(testDbOptions);
        await _testDbContext.Database.MigrateAsync();

        var mongoConnectionString = _mongoDbContainer.GetConnectionString();
        _mongoClient = new MongoClient(mongoConnectionString);
        _mongoDatabase = _mongoClient.GetDatabase("TestDB");

        var mongoConfig = new MovimientosMongoConfig
        {
            ConnectionString = mongoConnectionString,
            DatabaseName = "TestDB",
            MovimientosCollectionName = "Movimientos",
            DomiciliacionesCollectionName = "Domiciliaciones"
        };
        _movimientosDatabaseSettingsMock.Setup(x => x.Value).Returns(mongoConfig);
    }

    [Test]
    public async Task DirectorioOrigenNulo()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(
            () => _backupService.ExportToZip(null, "test.zip")
        );
        
        Assert.That(exception.ParamName, Is.EqualTo("sourceDirectory"));
    }

    [Test]
    public async Task RutaZipNula()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(
            () => _backupService.ExportToZip("origen", null)
        );
        
        Assert.That(exception.ParamName, Is.EqualTo("zipFilePath"));
    }
    
    [Test]
    public async Task ExportacionZipFileNotFound()
    {
        var dirOrigen = CreateTempDirectory();
        var rutaZip = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");

        try
        {
            File.Delete(Path.Combine(dirOrigen, "usuarios.json"));

            var exception = Assert.ThrowsAsync<ExportFromZipException>(
                async () => await _backupService.ExportToZip(dirOrigen, rutaZip)
            );

            Assert.That(exception.Message, Is.EqualTo("Ocurrió un error al intentar exportar datos al archivo ZIP."));
            Assert.That(exception.InnerException, Is.TypeOf<FileNotFoundException>());
            Assert.That(exception.InnerException.Message, Does.Contain("Could not find file"));
            Assert.That(exception.InnerException.Message, Does.Contain("usuarios.json"));
        }
        finally
        {
            CleanupFiles(rutaZip, dirOrigen);
        }
    }

    /*[Test]
    public async Task ExportacionZipDebeIncluirTodosLosArchivosJson()
    {
        var dirOrigen = CreateTempDirectory();
        var rutaZip = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");
        var archivosEsperados = new[] { 
            "usuarios.json", "clientes.json", "productos.json",
            "cuentas.json", "tarjetas.json", "movimientos.json", 
            "domiciliaciones.json" 
        };

        try
        {
            await _backupService.ExportToZip(dirOrigen, rutaZip);

            using var zip = ZipFile.OpenRead(rutaZip);
            var archivosEncontrados = zip.Entries.Select(e => e.Name).ToList();

            Assert.Multiple(() =>
            {
                foreach (var archivo in archivosEsperados)
                {
                    Assert.That(archivosEncontrados, Does.Contain(archivo));
                }
            });
        }
        finally
        {
            CleanupFiles(rutaZip, dirOrigen);
        }
    }*/

    [Test]
    public async Task GuardarEntidadesEvitaDuplicadosPorGuid()
    {
        var usuarioExistente = new UserEntity 
        { 
            Guid = "test-guid",
            Username = "test",
            Password = "test123",
            Role = Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var usuarioDuplicado = new UserEntity 
        { 
            Guid = "test-guid",
            Username = "duplicate",
            Password = "duplicate-pass",
            Role = Role.User,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _testDbContext.Usuarios.AddAsync(usuarioExistente);
        await _testDbContext.SaveChangesAsync();

        await _backupService.SaveEntidadesSiNoExistenAsync(
            new List<UserEntity> { usuarioDuplicado },
            _testDbContext.Usuarios,
            u => u.Id.ToString(),
            u => u.Guid
        );

        var usuariosCount = await _testDbContext.Usuarios.CountAsync();
        Assert.That(usuariosCount, Is.EqualTo(5));
    }

    [Test]
    public async Task GuardarEnMongoEvitaDuplicados()
    {
        var movimiento = new Movimiento 
        { 
            Id = "mov1",
            Guid = "guid1",
            ClienteGuid = "client1",
            Transferencia = new Transferencia
            {
                ClienteOrigen = "VAN3R3Q4EFKCDN",
                IbanOrigen = "ES12345678901234567890",
                IbanDestino = "ES98765432101234567890",
                Importe = 12233,
                Revocada = false,
                NombreBeneficiario = "fabio"
            },
            CreatedAt = DateTime.UtcNow
        };

        var movimientos = new List<Movimiento> { movimiento };

        _movimientosCollectionMock
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Movimiento>>(),
                It.IsAny<FindOptions<Movimiento, Movimiento>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new Mock<IAsyncCursor<Movimiento>>().Object);

        await _backupService.SaveSiNoExistenAsyncMongo(
            movimientos, 
            _movimientosCollectionMock.Object, 
            m => m.Guid
        );

        _movimientosCollectionMock.Verify(
            x => x.InsertManyAsync(
                It.IsAny<IEnumerable<Movimiento>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
    
    [Test]
    public async Task AgregarArchivosAlZip()
    {
        var dirOrigen = CreateTempDirectory();
        var rutaZip = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");

        var archivosJson = new[] { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" };
        foreach (var archivo in archivosJson)
        {
            File.WriteAllText(Path.Combine(dirOrigen, archivo), "{}");
        }

        var avatarDirectory = Path.Combine(dirOrigen, "data", "avatares");
        Directory.CreateDirectory(avatarDirectory);
        var avatarFilePath = Path.Combine(avatarDirectory, "avatar1.png");
        File.WriteAllText(avatarFilePath, "dummy content");

        Console.WriteLine($"Avatar directory exists: {Directory.Exists(avatarDirectory)}");
        Console.WriteLine($"Avatar file exists: {File.Exists(avatarFilePath)}");

        _backupService.AgregarArchivosAlZip(dirOrigen, rutaZip);

        using (var zip = ZipFile.OpenRead(rutaZip))
        {
            var archivosZip = zip.Entries.Select(entry => entry.Name).ToList();
            Assert.Multiple(() =>
            {
                foreach (var archivo in archivosJson)
                {
                    Assert.That(archivosZip, Does.Contain(archivo));
                }

                Assert.That(archivosZip, Does.Contain("clientes.json"));
                Assert.That(archivosZip, Does.Contain("productos.json"));
                Assert.That(archivosZip, Does.Contain("cuentas.json"));
                Assert.That(archivosZip, Does.Contain("tarjetas.json"));
                Assert.That(archivosZip, Does.Contain("movimientos.json"));
            });
        }

        CleanupFiles(rutaZip, dirOrigen);
    }

    [Test]
    public async Task EliminarArchivosTemporales()
    {
        var dirOrigen = CreateTempDirectory();
        var archivosJson = new[] { "usuarios.json", "clientes.json", "productos.json", "cuentas.json", "tarjetas.json", "movimientos.json", "domiciliaciones.json" };
        
        foreach (var archivo in archivosJson)
        {
            File.WriteAllText(Path.Combine(dirOrigen, archivo), "{}");
        }

        _backupService.EliminarArchivosTemporales(dirOrigen);

        foreach (var archivo in archivosJson)
        {
            var filePath = Path.Combine(dirOrigen, archivo);
            Assert.That(File.Exists(filePath), Is.False); 
        }

        Directory.Delete(dirOrigen, true);
    }
    
    [Test]
    public async Task ImportFromZip_WithValidZipFile_ImportsAllEntities()
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");
        var destinationDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        Directory.CreateDirectory(destinationDir);
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var files = new[] 
            { 
                "usuarios.json", "clientes.json", "productos.json",
                "cuentas.json", "tarjetas.json", "movimientos.json",
                "domiciliaciones.json" 
            };
            
            foreach (var file in files)
            {
                var entry = zip.CreateEntry(file);
                using var writer = new StreamWriter(entry.Open());
                writer.Write("[]");
            }
        }

        try
        {
            await _backupService.ImportFromZip(zipPath, destinationDir);
            
            _storageJsonMock.Verify(x => x.ImportJson<UserEntity>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<Banco_VivesBank.Cliente.Models.Cliente>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<ProductoEntity>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<Cuenta>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<Tarjeta>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<Movimiento>(It.IsAny<FileInfo>()), Times.Once);
            _storageJsonMock.Verify(x => x.ImportJson<Domiciliacion>(It.IsAny<FileInfo>()), Times.Once);

            var usersCount = await _testDbContext.Usuarios.CountAsync();
            Assert.That(usersCount, Is.GreaterThan(0));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Importación de datos desde ZIP finalizada")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
        finally
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(destinationDir)) Directory.Delete(destinationDir, true);
        }
    }

    /*[Test]
    public async Task ImportFromZip_WithMissingFiles_ThrowsException()
    {
        // Arrange
        var zipPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");
        var destinationDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        // Create a ZIP file with missing required files
        Directory.CreateDirectory(destinationDir);
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("usuarios.json"); // Only include one file
            using var writer = new StreamWriter(entry.Open());
            writer.Write("[]");
        }

        try
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ImportFromZipException>(
                async () => await _backupService.ImportFromZip(zipPath, destinationDir)
            );
            Assert.That(ex.Message, Does.Contain("Uno o más archivos necesarios"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(destinationDir)) Directory.Delete(destinationDir, true);
        }
    }*/

    [Test]
    public async Task ImportFromZip_WithInvalidZipPath_ThrowsException()
    {
        var invalidZipPath = Path.Combine(Path.GetTempPath(), "nonexistent.zip");
        var destinationDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var ex = Assert.ThrowsAsync<ImportFromZipException>(
            async () => await _backupService.ImportFromZip(invalidZipPath, destinationDir)
        );
        Assert.That(ex.Message, Does.Contain("El archivo ZIP no existe"));
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    private void CleanupFiles(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path)) File.Delete(path);
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_testDbContext != null)
        {
            await _testDbContext.DisposeAsync();
        }

        if (_mongoClient is IDisposable disposableClient)
        {
            disposableClient.Dispose();
        }

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        if (_mongoDbContainer != null)
        {
            await _mongoDbContainer.DisposeAsync();
        }
    }
    
    private void ConfigureStorageJsonMock()
    {
        _storageJsonMock
            .Setup(s => s.ImportJson<UserEntity>(It.IsAny<FileInfo>()))
            .Returns(new List<UserEntity> 
            { 
                new UserEntity 
                { 
                    Guid = "test-user-guid",
                    Username = "testuser",
                    Password = "testpass",
                    Role = Role.User
                }
            });

        _storageJsonMock
            .Setup(s => s.ImportJson<Banco_VivesBank.Cliente.Models.Cliente>(It.IsAny<FileInfo>()))
            .Returns(new List<Banco_VivesBank.Cliente.Models.Cliente> 
            { 
                /*new Banco_VivesBank.Cliente.Models.Cliente
                { 
                    Guid = "test-client-guid",
                    Nombre = "Test Client",
                    Email = "test@example.com",
                    Telefono = "1234567890",
                    Direccion = new Direccion
                    {
                        Calle = "Test Street",
                        Numero = "123",
                        CodigoPostal = "12345",
                        Piso = "2",
                        Letra = "A"
                    },
                    User = new Banco_VivesBank.User.Models.User
                    {
                        Guid = "test-user-guid",
                        Username = "testuser",
                        Password = "testpass",
                        Role = Role.User
                    },
                    Dni = "03177397Q",
                    Apellidos = "apellido"
                }*/
            });

        _storageJsonMock
            .Setup(s => s.ImportJson<ProductoEntity>(It.IsAny<FileInfo>()))
            .Returns(new List<ProductoEntity>());

        _storageJsonMock
            .Setup(s => s.ImportJson<Cuenta>(It.IsAny<FileInfo>()))
            .Returns(new List<Cuenta> ());

        _storageJsonMock
            .Setup(s => s.ImportJson<Tarjeta>(It.IsAny<FileInfo>()))
            .Returns(new List<Tarjeta>());

        _storageJsonMock
            .Setup(s => s.ImportJson<Movimiento>(It.IsAny<FileInfo>()))
            .Returns(new List<Movimiento>());

        _storageJsonMock
            .Setup(s => s.ImportJson<Domiciliacion>(It.IsAny<FileInfo>()))
            .Returns(new List<Domiciliacion>());
    }

}