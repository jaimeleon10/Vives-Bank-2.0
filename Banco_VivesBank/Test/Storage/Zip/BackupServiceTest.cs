using System.IO.Compression;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.Movimientos.Database;
using Banco_VivesBank.Storage.Json.Service;
using Banco_VivesBank.Storage.Zip.Services;
using Banco_VivesBank.Movimientos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;

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
                Directory.CreateDirectory(file.DirectoryName ?? throw new ArgumentNullException(nameof(file.DirectoryName)));
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
    public void ExportToZip_ShouldThrowArgumentNullException_WhenSourceDirectoryIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.ExportToZip(null, "test.zip"));
    }

    [Test]
    public void ExportToZip_ShouldThrowArgumentNullException_WhenZipFilePathIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.ExportToZip("source", null));
    }

    [Test]
    public async Task ExportToZip_ShouldCreateZipFile_WhenSourceDirectoryExists()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");

        // Act
        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Assert
        Assert.That(File.Exists(zipFilePath), Is.True);
        Cleanup(zipFilePath, sourceDirectory);
    }

    [Test]
    public async Task ExportToZip_ShouldIncludeAllJsonFilesInZip()
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
    public async Task ExportToZip_ShouldOverwriteExistingZipFile()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        File.WriteAllText(zipFilePath, "dummy content");

        // Act
        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Assert
        using var zip = ZipFile.OpenRead(zipFilePath);
        Assert.That(zip.Entries.Count, Is.GreaterThan(0));
        Cleanup(zipFilePath, sourceDirectory);
    }

    [Test]
    public async Task ExportToZip_ShouldIncludeAvatarsDirectory_WhenExists()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var avatarDir = Path.Combine("data", "avatares");
        Directory.CreateDirectory(avatarDir);
        File.WriteAllText(Path.Combine(avatarDir, "test.png"), "dummy");

        // Act
        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Assert
        using var zip = ZipFile.OpenRead(zipFilePath);
        Assert.That(zip.Entries.Any(e => e.FullName.StartsWith("avatares/")), Is.True);
        Cleanup(zipFilePath, sourceDirectory, avatarDir);
    }

    [Test]
    public async Task ImportFromZip_ShouldThrowImportFromZipException_WhenZipFileDoesNotExist()
    {
        var zipFilePath = Path.Combine(Path.GetTempPath(), "nonexistent.zip");
        var destinationDirectory = CreateTempDirectory();

        Assert.ThrowsAsync<ImportFromZipException>(() => 
            _backupService.ImportFromZip(zipFilePath, destinationDirectory));
    }

    [Test]
    public async Task ImportFromZip_ShouldImportAllEntities_WhenZipIsValid()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();

        // Populate test data
        await _dbContext.Usuarios.AddAsync(new UserEntity { Guid = Guid.NewGuid().ToString() });
        await _dbContext.SaveChangesAsync();

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Act
        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        // Assert
        Assert.That(await _dbContext.Usuarios.AnyAsync(), Is.True);
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory);
    }

    [Test]
    public async Task ImportFromZip_ShouldSkipExistingEntities()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();

        var originalUser = new UserEntity { Guid = "test-guid" };
        await _dbContext.Usuarios.AddAsync(originalUser);
        await _dbContext.SaveChangesAsync();

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);
        _dbContext.Usuarios.Remove(originalUser);
        await _dbContext.SaveChangesAsync();

        // Act
        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        // Assert
        Assert.That(await _dbContext.Usuarios.CountAsync(), Is.EqualTo(1));
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory);
    }

    [Test]
    public async Task ImportFromZip_ShouldHandleEmptyCollections()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();

        // Act & Assert (no debería lanzar excepciones)
        await _backupService.ExportToZip(sourceDirectory, zipFilePath);
        Assert.DoesNotThrowAsync(() => _backupService.ImportFromZip(zipFilePath, destinationDirectory));
        
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory);
    }

    [Test]
    public async Task ImportFromZip_ShouldCopyAvatarsToDestination()
    {
        // Arrange
        var sourceDirectory = CreateTempDirectory();
        var zipFilePath = Path.Combine(Path.GetTempPath(), "test.zip");
        var destinationDirectory = CreateTempDirectory();
        var avatarPath = Path.Combine("data", "avatares", "test.png");
        Directory.CreateDirectory(Path.GetDirectoryName(avatarPath));
        File.WriteAllText(avatarPath, "dummy");

        await _backupService.ExportToZip(sourceDirectory, zipFilePath);

        // Act
        await _backupService.ImportFromZip(zipFilePath, destinationDirectory);

        // Assert
        Assert.That(File.Exists(avatarPath), Is.True);
        Cleanup(zipFilePath, sourceDirectory, destinationDirectory, avatarPath);
    }

    /*[Test]
    public async Task SaveEntidadesSiNoExistenAsync_ShouldSkipExistingRecords()
    {
        // Arrange
        var existingUser = new UserEntity { Guid = "existing-guid" };
        await _dbContext.Usuarios.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var newUser = new UserEntity { Guid = "new-guid" };
        var users = new List<UserEntity> { existingUser, newUser };

        // Act
        await _backupService.SaveEntidadesSiNoExistenAsync(
            users,
            _dbContext.Usuarios,
            u => u.Id.ToString(),
            u => u.Guid
        );

        // Assert
        Assert.That(await _dbContext.Usuarios.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task SaveSiNoExistenAsyncMongo_ShouldInsertOnlyNewDocuments()
    {
        // Arrange
        var existingDoc = new Movimiento { Guid = "existing-guid" };
        var newDoc = new Movimiento { Guid = "new-guid" };
        var documents = new List<Movimiento> { existingDoc, newDoc };

        _mockMovimientosCollection.Setup(x => x.Find(It.IsAny<FilterDefinition<Movimiento>>(), null))
            .ReturnsAsync(new List<Movimiento> { existingDoc });

        // Act
        await _backupService.SaveSiNoExistenAsyncMongo(
            documents,
            _mockMovimientosCollection.Object,
            m => m.Guid
        );

        // Assert
        _mockMovimientosCollection.Verify(x => 
            x.InsertManyAsync(It.Is<List<Movimiento>>(l => l.Count == 1), null), Times.Once);
    }*/

    // Helpers
    private string CreateTempDirectory() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    
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
}