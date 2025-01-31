using Banco_VivesBank.Storage.Zip.Controller;
using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.Storage.Zip.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Storage.Zip;

[TestFixture]
public class BackupControllerTests
{
    private Mock<ILogger<BackupService>> _loggerMock;
    private Mock<IBackupService> _backupServiceMock;
    private BackupController _controller;
    private string _testDataDirectory;
    private string _testZipPath;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _backupServiceMock = new Mock<IBackupService>();

        _testDataDirectory = Path.Combine(Path.GetTempPath(), "test_backup_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDataDirectory);
        _testZipPath = Path.Combine(_testDataDirectory, "backup.zip");

        _controller = new BackupController(
            _loggerMock.Object, 
            _backupServiceMock.Object
        );
        
        typeof(BackupController)
            .GetField("_dataDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_controller, _testDataDirectory);
        
        typeof(BackupController)
            .GetField("_backupFilePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_controller, _testZipPath);
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_testDataDirectory))
            Directory.Delete(_testDataDirectory, true);
    }

    [Test]
    public async Task ExportToZip()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.Not.Null, "El resultado es nulo.");
        Assert.That(result, Is.InstanceOf<OkResult>(), "El resultado no es un OkResult como se esperaba.");
        _backupServiceMock.Verify(
            s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once, "El método ExportToZip no fue invocado correctamente."
        );
    }

    [Test]
    public async Task ExportToZipDirectorioNoEncontrado()
    {
        Directory.Delete(_testDataDirectory, true);

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("El directorio fuente no existe."));
    }

    [Test]
    public async Task ExportToZipArgumentException()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentNullException("test"));

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("Argumentos de exportación inválidos."));
    }

    [Test]
    public async Task ExportToZipIOException()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("test"));

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error de archivo al exportar."));
    }

    [Test]
    public async Task ExportToZipException()
    {
        _backupServiceMock
            .Setup(s => s.ExportToZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ExportToZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error al exportar datos"));
    }

    [Test]
    public async Task ImportFromZip()
    {
        File.WriteAllText(_testZipPath, "test content");
        _backupServiceMock
            .Setup(s => s.ImportFromZip(
                It.Is<string>(path => path == _testZipPath), 
                It.Is<string>(dir => dir == _testDataDirectory)
            ))
            .Returns(Task.CompletedTask);

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.Not.Null, "El resultado es nulo.");
        Assert.That(result, Is.InstanceOf<OkResult>(), "El resultado no es un OkResult.");
        _backupServiceMock.Verify(
            s => s.ImportFromZip(_testZipPath, _testDataDirectory),
            Times.Once, "El método ImportFromZip no fue invocado correctamente."
        );
    }

    [Test]
    public async Task ImportFromZipArchivoNotFound()
    {
        if (File.Exists(_testZipPath))
            File.Delete(_testZipPath);

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("El archivo ZIP no existe."));
    }

    [Test]
    public async Task ImportFromZipImportException()
    {
        File.WriteAllText(_testZipPath, "test content");
        _backupServiceMock
            .Setup(s => s.ImportFromZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ImportFromZipException("Test exception", null));

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo("Los archivos dentro del ZIP no son válidos o están incompletos."));
    }

    [Test]
    public async Task ImportFromZipIOException()
    {
        File.WriteAllText(_testZipPath, "test content");
        _backupServiceMock
            .Setup(s => s.ImportFromZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("Test exception"));

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error de archivo al importar."));
    }

    [Test]
    public async Task ImportFromZipException()
    {
        File.WriteAllText(_testZipPath, "test content");
        _backupServiceMock
            .Setup(s => s.ImportFromZip(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ImportFromZip();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("Error al importar datos"));
    }
}