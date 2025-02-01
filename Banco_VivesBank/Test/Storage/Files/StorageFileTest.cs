using Banco_VivesBank.Config.Storage;
using Banco_VivesBank.Config.Storage.Images;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Images.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Storage.Images;

public class StorageFileTest
{
    private Mock<ILogger<FileStorageService>> _logger;
    private Mock<IOptions<FileStorageConfig>> _storageConfig;
    private FileStorageService _storageService;

    public StorageFileTest()
    {
        _logger = new Mock<ILogger<FileStorageService>>();
        var fileStorageConfig = new FileStorageConfig
        {
            MaxFileSize = 5 * 1024 * 1024,
            AllowedFileTypes = new List<string> { ".jpg", ".png" },
            UploadDirectory = Path.Combine(Path.GetTempPath(), "Uploads")
        };

        _storageConfig = new Mock<IOptions<FileStorageConfig>>();
        _storageConfig.Setup(o => o.Value).Returns(fileStorageConfig);

        _storageService = new FileStorageService(_storageConfig.Object, _logger.Object);
    }

    [Test]
    public async Task SaveFileAsync()
    {
        var mockFile = new Mock<IFormFile>();
        var fileName = "test.png";
        var content = "Test content";

        mockFile.Setup(f => f.FileName).Returns(fileName);

        var result = await _storageService.SaveFileAsync(mockFile.Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(File.Exists(Path.Combine(_storageConfig.Object.Value.UploadDirectory, result)), Is.True);

        File.Delete(Path.Combine(_storageConfig.Object.Value.UploadDirectory, result));
    }

    [Test]
    public void GetFileAsyncErrorAlObtenerArchivo()
    {
        var fileName = "existentfile.png";
        _storageService = new FileStorageService(_storageConfig.Object, _logger.Object);

        _logger.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                (logLevel, eventId, state, exception, formatter) =>
                {
                    throw new IOException("Error reading file.");
                });

        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.GetFileAsync(fileName));
        Assert.That(exception.Message, Is.EqualTo("File not found: existentfile.png"));
    }

    [Test]
    public void DeleteFileAsyncErrorAlEliminarArchivo()
    {
        var fileName = "existentfile.png";
        _storageService = new FileStorageService(_storageConfig.Object, _logger.Object);

        _logger.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                (logLevel, eventId, state, exception, formatter) =>
                {
                    throw new IOException("Error deleting file.");
                });

        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.DeleteFileAsync(fileName));
        Assert.That(exception.Message, Is.EqualTo("File not found: existentfile.png"));
    }
    
    [Test]
    public void SaveFileAsyncFicheroGrande()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(_storageConfig.Object.Value.MaxFileSize + 1);

        var exception = Assert.ThrowsAsync<FileStorageException>(() => _storageService.SaveFileAsync(mockFile.Object));
        Assert.That(exception.Message, Is.EqualTo("El tamaño del fichero excede el máximo permitido."));
    }
    
    [Test]
    public void SaveFileAsyncFicheroInvalido()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("file.exe");

        var exception = Assert.ThrowsAsync<FileStorageException>(() => _storageService.SaveFileAsync(mockFile.Object));
        Assert.That(exception.Message, Is.EqualTo("Tipo de fichero no permitido."));
    }
    
    [Test]
    public async Task GetFileAsync()
    {
        var fileName = "test1.png";
        var path = Path.Combine(_storageConfig.Object.Value.UploadDirectory, fileName);

        Directory.CreateDirectory(_storageConfig.Object.Value.UploadDirectory);
        File.WriteAllText(path, "Test content");

        var result = await _storageService.GetFileAsync(fileName);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<FileStream>());

        result.Close();

        File.Delete(path);
    }
    
    [Test]
    public void GetFileAsyncFileNotFound()
    {
        var fileName = "nonexistent.png";

        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.GetFileAsync(fileName));

        Assert.That(exception.Message, Is.EqualTo($"File not found: {fileName}"));
    }
    
    [Test]
    public async Task DeleteFileAsync()
    {
        var fileName = "existentfile.png";
        var filePath = Path.Combine(_storageConfig.Object.Value.UploadDirectory, fileName);

        Directory.CreateDirectory(_storageConfig.Object.Value.UploadDirectory);
        File.WriteAllText(filePath, "Test content");
        
        var result = await _storageService.DeleteFileAsync(fileName);

        Assert.That(result, Is.True);

        File.Delete(filePath);
    }
    
    [Test]
    public void DeleteFileAsyncFileNotFound()
    {
        var fileName = "nonexistentfile.png";
        
        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.DeleteFileAsync(fileName));
        Assert.That(exception.Message, Is.EqualTo($"File not found: {fileName}") );
    }
}