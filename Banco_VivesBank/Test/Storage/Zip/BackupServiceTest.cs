using System.IO.Compression;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Services;
using Banco_VivesBank.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
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

    private void ConfigureStorageJsonMock()
    {
        _storageJsonMock
            .Setup(s => s.ExportJson(It.IsAny<FileInfo>(), It.IsAny<List<object>>()))
            .Callback<FileInfo, List<object>>((file, data) =>
            {
                Directory.CreateDirectory(file.DirectoryName!);
                File.WriteAllText(file.FullName, JsonConvert.SerializeObject(data));
            });
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
    public async Task ExportacionZip()
    {
        var dirOrigen = CreateTempDirectory();
        var rutaZip = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");

        try
        {
            await _backupService.ExportToZip(dirOrigen, rutaZip);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(rutaZip), Is.True);
                Assert.That(new FileInfo(rutaZip).Length, Is.GreaterThan(0));
            });
        }
        finally
        {
            CleanupFiles(rutaZip, dirOrigen);
        }
    }

    [Test]
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
    }
    
    [Test]
    public async Task ImportarZip()
    {
        var dirOrigen = CreateTempDirectory();
        var rutaZip = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.zip");
        var dirDestino = CreateTempDirectory();

        try
        {
            await _backupService.ExportToZip(dirOrigen, rutaZip);
            await _backupService.ImportFromZip(rutaZip, dirDestino);

            var archivosDirectorio = Directory.Exists(Path.Combine(dirDestino, "archivos"));
            Assert.That(archivosDirectorio, Is.True);
        }
        finally
        {
            CleanupFiles(rutaZip, dirOrigen, dirDestino);
        }
    }

    [Test]
    public async Task GuardarEntidadesEvitaDuplicados()
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
        
        await _testDbContext.Usuarios.AddAsync(usuarioExistente);
        await _testDbContext.SaveChangesAsync();

        await _backupService.SaveEntidadesSiNoExistenAsync(
            new List<UserEntity> { usuarioExistente },
            _testDbContext.Usuarios,
            u => u.Id.ToString(),
            u => u.Guid
        );

        var usuariosCount = await _testDbContext.Usuarios.CountAsync();
        Assert.That(usuariosCount, Is.EqualTo(1));
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
}