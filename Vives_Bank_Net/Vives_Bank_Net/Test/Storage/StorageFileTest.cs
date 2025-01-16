using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Vives_Bank_Net.Config.Storage;
using Vives_Bank_Net.Storage;
using Vives_Bank_Net.Storage.Exceptions;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace Vives_Bank_Net.Test;

public class StorageFileTest
{
    private readonly Mock<ILogger<FileStorageService>> _logger;
    private readonly Mock<IOptions<FileStorageConfig>> _storageConfig;
    private readonly FileStorageService _storageService;

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
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var memoryStream = new MemoryStream(contentBytes);

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(contentBytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Callback<Stream, CancellationToken>((stream, _) => memoryStream.CopyTo(stream))
            .Returns(Task.CompletedTask);

        var result = await _storageService.SaveFileAsync(mockFile.Object);

        Assert.That(result, Is.Not.Null);
        Assert.That(File.Exists(Path.Combine(_storageConfig.Object.Value.UploadDirectory, result)), Is.True);

        File.Delete(Path.Combine(_storageConfig.Object.Value.UploadDirectory, result));
    }
    
    [Test]
    public void SaveFileAsyncFicheroGrande()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("largefile.png");
        mockFile.Setup(f => f.Length).Returns(_storageConfig.Object.Value.MaxFileSize + 1);

        var exception = Assert.ThrowsAsync<FileStorageException>(() => _storageService.SaveFileAsync(mockFile.Object));
        Assert.That(exception.Message, Is.EqualTo("El tamaño del fichero excede el máximo permitido."));
    }
    
    [Test]
    public void SaveFileAsyncFicheroInvalido()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("file.exe");
        mockFile.Setup(f => f.Length).Returns(1024);

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
        var fileName = "nonexistent.txt";

        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.GetFileAsync(fileName));

        Assert.That(exception.Message, Is.EqualTo($"File not found: {fileName}"));
    }
    
    [Test]
    public async Task DeleteFileAsync()
    {
        var fileName = "existentfile.txt";
        var filePath = Path.Combine(_storageConfig.Object.Value.UploadDirectory, fileName);

        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Uploads"));
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
    
    [Test]
    public async Task DeleteFileAsyncFileStorageException()
    {
        var fileName = "nonexistentfile.txt";

        var exception = Assert.ThrowsAsync<FileStorageNotFoundException>(() => _storageService.DeleteFileAsync(fileName));

        Assert.That(exception.Message, Is.EqualTo($"File not found: {fileName}"));
    }
}