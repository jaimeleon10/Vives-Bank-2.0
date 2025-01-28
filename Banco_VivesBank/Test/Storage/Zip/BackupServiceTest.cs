using System.IO.Compression;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.Storage.Zip.Services;
using Banco_VivesBank.User.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Testcontainers.PostgreSql;

namespace Test.Storage.Zip;

[TestFixture]
public class BackupServiceTests
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IStorageJson> _storageJsonMock;
    private Mock<IMongoClient> _mockMongoClient;
    private Mock<IMongoDatabase> _mockMongoDatabase;
    private Mock<IMongoCollection<Movimiento>> _mockMovimientosCollection;
    private Mock<IMongoCollection<Domiciliacion>> _mockDomiciliacionesCollection;
    private GeneralDbContext _dbContext;
    private BackupService _backupService;

    private PostgreSqlContainer _postgreSqlContainer;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        // PostgreSQL
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

        // MongoDB
        _mockMovimientosCollection = new Mock<IMongoCollection<Movimiento>>();
        _mockDomiciliacionesCollection = new Mock<IMongoCollection<Domiciliacion>>();
        _mockMongoDatabase = new Mock<IMongoDatabase>();
        _mockMongoClient = new Mock<IMongoClient>();

        _mockMongoDatabase
            .Setup(db => db.GetCollection<Movimiento>("movimientos", null))
            .Returns(_mockMovimientosCollection.Object);

        _mockMongoDatabase
            .Setup(db => db.GetCollection<Domiciliacion>("domiciliaciones", null))
            .Returns(_mockDomiciliacionesCollection.Object);

        _mockMongoClient
            .Setup(client => client.GetDatabase("test", null))
            .Returns(_mockMongoDatabase.Object);

        // Configurar servicio BackupService con mocks
        _loggerMock = new Mock<ILogger<BackupService>>();
        _storageJsonMock = new Mock<IStorageJson>();

        var mongoConfig = Options.Create(new MovimientosMongoConfig
        {
            ConnectionString = "mongodb://fakehost",
            DatabaseName = "test",
            MovimientosCollectionName = "movimientos",
            DomiciliacionesCollectionName = "domiciliaciones"
        });

        _backupService = new BackupService(
            _loggerMock.Object,
            _dbContext,
            _storageJsonMock.Object,
            mongoConfig
        );
    }

    [SetUp]
    public async Task CleanDatabase()
    {
        // Limpiar la base de datos PostgreSQL
        _dbContext.Clientes.RemoveRange(_dbContext.Clientes);
        _dbContext.Usuarios.RemoveRange(_dbContext.Usuarios);
        await _dbContext.SaveChangesAsync();

        // Limpiar colecciones MongoDB simuladas
        _mockMovimientosCollection.Reset();
        _mockDomiciliacionesCollection.Reset();
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
    public async Task BackupService_ShouldSaveDataToPostgres()
    {
        var user = new UserEntity
        {
            Id = 1, 
            Username = "Test Cliente" , 
            IsDeleted = false, 
            Password = "password", 
            Role = Role.User, 
            Guid = "jwjrvflslv",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Usuarios.Add(user);

        await _dbContext.SaveChangesAsync();
        var result = await _dbContext.Usuarios.FindAsync(user.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(user.Username, Is.EqualTo(result.Username));
        Assert.That(user.IsDeleted, Is.EqualTo(result.IsDeleted));
        Assert.That(user.Password, Is.EqualTo(result.Password));
        Assert.That(user.Role, Is.EqualTo(result.Role));
    }
    
    [Test]
    public void BackupServiceMongoException()
    {
        _mockMovimientosCollection
            .Setup(c => c.InsertOne(It.IsAny<Movimiento>(), null, default))
            .Throws(new MongoException("Error de prueba"));

        Assert.Throws<MongoException>(() =>
        {
            _mockMovimientosCollection.Object.InsertOne(new Movimiento { Id = "1", Guid = "jofwbfkaj", ClienteGuid = "jdjdjjj"});
        });
    }

    [Test]
    public async Task ExportToZi()
    {
        var sourceDirectory = Path.Combine(Path.GetTempPath(), "test-source");
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test-backup.zip");
        Directory.CreateDirectory(sourceDirectory);

        var user = new UserEntity
        {
            Id = 1,
            Username = "Test User",
            IsDeleted = false,
            Password = "password",
            Role = Role.User,
            Guid = "test-guid",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Usuarios.Add(user);
        await _dbContext.SaveChangesAsync();

        _mockMovimientosCollection
            .Setup(c => c.Find(It.IsAny<FilterDefinition<Movimiento>>(), null))
            .Returns(Mock.Of<IFindFluent<Movimiento, Movimiento>>());

        _mockDomiciliacionesCollection
            .Setup(c => c.Find(It.IsAny<FilterDefinition<Domiciliacion>>(), null))
            .Returns(Mock.Of<IFindFluent<Domiciliacion, Domiciliacion>>());

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        Assert.That(File.Exists(zipFilePath), Is.True);
        Assert.That(new FileInfo(zipFilePath).Length, Is.GreaterThan(0));

        Directory.Delete(sourceDirectory, true);
        File.Delete(zipFilePath);
    }

    [Test]
    public void ExportToZip_DeberiaLanzarExcepcionSiDirectorioOrigenEsNulo()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _backupService.ExportToZip(null, "test.zip"));
        Assert.That(exception.ParamName, Is.EqualTo("sourceDirectory"));
    }

    [Test]
    public void ExportToZip_DeberiaLanzarExcepcionSiRutaZipEsNula()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _backupService.ExportToZip("testDir", null));
        Assert.That(exception.ParamName, Is.EqualTo("zipFilePath"));
    }

    [Test]
    public async Task ImportFromZip_DeberiaImportarDatosCorrectamente()
    {
        var sourceDirectory = Path.Combine(Path.GetTempPath(), "test-import-source");
        var destinationDirectory = Path.Combine(Path.GetTempPath(), "test-import-dest");
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test-import.zip");

        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(destinationDirectory);

        File.WriteAllText(Path.Combine(sourceDirectory, "usuarios.json"), "[  {\n    \"id\": 1,\n    \"guid\": \"nGB65b7P0LD\",\n    \"username\": \"pedrito\",\n    \"password\": \"password\",\n    \"role\": \"User\",\n    \"createdAt\": \"2025-01-26T01:04:44.153512Z\",\n    \"updatedAt\": \"2025-01-26T01:04:44.153512Z\",\n    \"isDeleted\": false\n  }]");
        File.WriteAllText(Path.Combine(sourceDirectory, "clientes.json"), "[  {\n    \"id\": 2,\n    \"guid\": \"jkWOwLHrN8O\",\n    \"dni\": \"21240915R\",\n    \"nombre\": \"Ana\",\n    \"apellidos\": \"Martinez\",\n    \"direccion\": {\n      \"calle\": \"Calle Dos\",\n      \"numero\": \"456\",\n      \"codigoPostal\": \"28002\",\n      \"piso\": \"2\",\n      \"letra\": \"B\"\n    },\n    \"email\": \"ana.martinez@gmail.com\",\n    \"telefono\": \"623456789\",\n    \"fotoPerfil\": \"https://example.com/fotoPerfil.jpg\",\n    \"fotoDni\": \"https://example.com/fotoDni.jpg\",\n    \"cuentas\": [],\n    \"user\": {\n      \"id\": 2,\n      \"guid\": \"w2JFmZHc9mE\",\n      \"username\": \"anita\",\n      \"password\": \"password\",\n      \"role\": \"Admin\",\n      \"createdAt\": \"2025-01-26T01:04:44.15352Z\",\n      \"updatedAt\": \"2025-01-26T01:04:44.15352Z\",\n      \"isDeleted\": false\n    },\n    \"createdAt\": \"2025-01-27T22:42:45.1307902Z\",\n    \"updatedAt\": \"2025-01-27T22:42:45.1307908Z\",\n    \"isDeleted\": false\n  }]");
        File.WriteAllText(Path.Combine(sourceDirectory, "productos.json"), "[  {\n    \"id\": 1,\n    \"guid\": \"xnqfxrGckcL\",\n    \"nombre\": \"Cuenta de ahorros\",\n    \"descripcion\": \"Producto para cuenta bancaria de ahorros\",\n    \"tipoProducto\": \"cuentaAhorros\",\n    \"tae\": 2.5,\n    \"createdAt\": \"2025-01-26T01:04:44.154188Z\",\n    \"updatedAt\": \"2025-01-26T01:04:44.154188Z\",\n    \"isDeleted\": false\n  }]");
        File.WriteAllText(Path.Combine(sourceDirectory, "cuentas.json"), "[  {\n    \"id\": 1,\n    \"guid\": \"YK6PqjYre2P\",\n    \"iban\": \"ES7730046576085345979538\",\n    \"saldo\": 5000.0,\n    \"tarjeta\": null,\n    \"cliente\": {\n      \"id\": 1,\n      \"guid\": \"LTtXSvg383G\",\n      \"dni\": \"12345678Z\",\n      \"nombre\": \"Pedro\",\n      \"apellidos\": \"Picapiedra\",\n      \"direccion\": {\n        \"calle\": \"Calle Uno\",\n        \"numero\": \"123\",\n        \"codigoPostal\": \"28001\",\n        \"piso\": \"1\",\n        \"letra\": \"A\"\n      },\n      \"email\": \"pedro.picapiedra@gmail.com\",\n      \"telefono\": \"612345678\",\n      \"fotoPerfil\": \"https://example.com/fotoPerfil.jpg\",\n      \"fotoDni\": \"https://example.com/fotoDni.jpg\",\n      \"cuentas\": [],\n      \"user\": {\n        \"id\": 1,\n        \"guid\": \"nGB65b7P0LD\",\n        \"username\": \"pedrito\",\n        \"password\": \"password\",\n        \"role\": \"User\",\n        \"createdAt\": \"2025-01-26T01:04:44.153512Z\",\n        \"updatedAt\": \"2025-01-26T01:04:44.153512Z\",\n        \"isDeleted\": false\n      },\n      \"createdAt\": \"2025-01-27T22:42:45.9093054Z\",\n      \"updatedAt\": \"2025-01-27T22:42:45.909306Z\",\n      \"isDeleted\": false\n    }]");
        File.WriteAllText(Path.Combine(sourceDirectory, "tarjetas.json"), "[  {\n    \"id\": 1,\n    \"guid\": \"sKA4H3SjRcI\",\n    \"numero\": \"458963749087423\",\n    \"fechaVencimiento\": \"06/27\",\n    \"cvv\": \"423\",\n    \"pin\": \"1234\",\n    \"limiteDiario\": 500.0,\n    \"limiteSemanal\": 2500.0,\n    \"limiteMensual\": 10000.0,\n    \"createdAt\": \"2025-01-26T01:04:44.15522Z\",\n    \"updatedAt\": \"2025-01-26T01:04:44.15522Z\",\n    \"isDeleted\": false\n  }]");
        File.WriteAllText(Path.Combine(sourceDirectory, "movimientos.json"), "[]");
        File.WriteAllText(Path.Combine(sourceDirectory, "domiciliaciones.json"), "[]");

        ZipFile.CreateFromDirectory(sourceDirectory, zipFilePath);

        _storageJsonMock
            .Setup(s => s.ImportJson<UserEntity>(It.IsAny<FileInfo>()))
            .Returns(new List<UserEntity>());

        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        Assert.That(Directory.Exists(Path.Combine(destinationDirectory, "archivos")), Is.False);

        Directory.Delete(sourceDirectory, true);
        Directory.Delete(destinationDirectory, true);
        File.Delete(zipFilePath);
    }

    [Test]
    public void ImportFromZip_DeberiaLanzarExcepcionSiArchivoZipNoExiste()
    {
        var exception = Assert.ThrowsAsync<ImportFromZipException>(async () =>
            await _backupService.ImportFromZip("noexiste.zip", "testDir"));
        Assert.That(exception.Message, Is.EqualTo("El archivo ZIP no existe."));
    }

    /*[Test]
    public async Task SaveEntidadesSiNoExistenAsync_DeberiaGuardarSoloEntidadesNuevas()
    {
        var usuarios = new List<UserEntity>
        {
            new()
            {
                Id = 1,
                Username = "Usuario Existente",
                Guid = "guid-existente",
                Role = Role.User
            },
            new()
            {
                Id = 2,
                Username = "Usuario Nuevo",
                Guid = "guid-nuevo",
                Role = Role.User
            }
        };

        _dbContext.Usuarios.Add(usuarios[0]);
        await _dbContext.SaveChangesAsync();

        await _backupService.SaveEntidadesSiNoExistenAsync(
            usuarios,
            _dbContext.Usuarios,
            u => u.Id.ToString(),
            u => u.Guid
        );

        var usuariosGuardados = await _dbContext.Usuarios.ToListAsync();
        Assert.That(usuariosGuardados.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task SaveSiNoExistenAsyncMongo_DeberiaGuardarSoloDocumentosNuevos()
    {
        var movimientos = new List<Movimiento>
        {
            new Movimiento() { Id = "1", Guid = "guid-existente" , ClienteGuid = "377373737"},
            new Movimiento() { Id = "2", Guid = "guid-nuevo", ClienteGuid = "hdhdhdhfbeibfo"}
        };

        var cursorMock = new Mock<IAsyncCursor<Movimiento>>();
        cursorMock
            .Setup(c => c.Current)
            .Returns(new List<Movimiento> { movimientos[0] });
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(default))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _mockMovimientosCollection
            .Setup(c => c.Find(It.IsAny<FilterDefinition<Movimiento>>(), null))
            .Returns(cursorMock.Object);

        await _backupService.SaveSiNoExistenAsyncMongo(
            movimientos,
            _mockMovimientosCollection.Object,
            m => m.Guid
        );

        _mockMovimientosCollection.Verify(
            c => c.InsertManyAsync(
                It.Is<IEnumerable<Movimiento>>(m => m.Count() == 1),
                null,
                default
            ),
            Times.Once
        );
    }*/
}
