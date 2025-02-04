using System.Net;
using System.Text;
using Banco_VivesBank.Config.Storage;
using Banco_VivesBank.Config.Storage.Ftp;
using Banco_VivesBank.Storage.Ftp.Exceptions;
using Banco_VivesBank.Storage.Ftp.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Storage.Ftp;

[TestFixture]
public class FtpServiceTests 
{
    private FtpService _ftpService;
    private Mock<ILogger<FtpService>> _mockLogger;
    private Mock<IOptions<FtpConfig>> _mockFtpConfig;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<FtpService>>();
        _mockFtpConfig = new Mock<IOptions<FtpConfig>>();
        _mockFtpConfig.Setup(x => x.Value).Returns(new FtpConfig
        {
            Host = "localhost",
            Port = 21,
            Username = "mockUser",
            Password = "mockPassword"
        });

        var mockFtpService = new Mock<IFtpService>();

        mockFtpService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mockFtpService.Setup(s => s.CheckFileExiste(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockFtpService.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _ftpService = new FtpService(_mockLogger.Object, _mockFtpConfig.Object);
    }

    [Test]
    public async Task UploadFile()
    {
        string uploadPath = "data/test.jpg";
        var testContent = Encoding.UTF8.GetBytes("test file content");
        using var inputStream = new MemoryStream(testContent);

        await _ftpService.UploadFileAsync(inputStream, uploadPath);

        var checkResult = await _ftpService.CheckFileExiste(uploadPath);
        Assert.That(checkResult, Is.True);
    }

    [Test]
    public async Task DownloadFile()
    {
        string uploadPath = "data/download-test.jpg";
        var testContent = Encoding.UTF8.GetBytes("download test content");
        using var uploadStream = new MemoryStream(testContent);
        await _ftpService.UploadFileAsync(uploadStream, uploadPath);

        string localFilePath = Path.GetTempFileName();

        await _ftpService.DownloadFileAsync(uploadPath, localFilePath);

        Assert.That(File.Exists(localFilePath), Is.True);
        var downloadedContent = await File.ReadAllBytesAsync(localFilePath);
        Assert.That(downloadedContent, Is.EqualTo(testContent));
    }

    [Test]
    public async Task DeleteFile()
    {
        string uploadPath = "data/delete-test.jpg";
        var testContent = Encoding.UTF8.GetBytes("delete test content");
        using var uploadStream = new MemoryStream(testContent);
        await _ftpService.UploadFileAsync(uploadStream, uploadPath);

        await _ftpService.DeleteFileAsync(uploadPath);

        var checkResult = await _ftpService.CheckFileExiste(uploadPath);
        Assert.That(checkResult, Is.False);
    }
    
    [Test]
    public void UploadFileAsyncInvalidMimeType()
    {
        var mockInputStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
        string uploadPath = "data/test.invalidext";

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _ftpService.UploadFileAsync(mockInputStream, uploadPath));
        
        Assert.That(ex.Message, Does.Contain("tipo MIME para la extensión"));
    }
    
    [Test]
    public void DownloadFileAsyncFtpException()
    {
        var mockInputStream = new MemoryStream();
        string remoteFilePath = "data/nonexistent-file.jpg";
        string localFilePath = Path.GetTempFileName();
    
        _mockFtpConfig.Setup(x => x.Value).Returns(new FtpConfig
        {
            Host = "localhost",
            Port = 21,
            Username = "mockUser",
            Password = "mockPassword"
        });

        _ftpService = new FtpService(_mockLogger.Object, _mockFtpConfig.Object);

        var ex = Assert.ThrowsAsync<FtpException>(async () => 
            await _ftpService.DownloadFileAsync(remoteFilePath, localFilePath));
    
        Assert.That(ex.Message, Is.EqualTo("Error al descargar el archivo desde el servidor FTP."));
    }

    [Test]
    public void DeleteFileFtpException()
    {
        string remoteFilePath = "data/nonexistent-file.jpg";

        _mockFtpConfig.Setup(x => x.Value).Returns(new FtpConfig
        {
            Host = "localhost",
            Port = 21,
            Username = "mockUser",
            Password = "mockPassword"
        });

        _ftpService = new FtpService(_mockLogger.Object, _mockFtpConfig.Object);

        var ex = Assert.ThrowsAsync<FtpException>(async () => 
            await _ftpService.DeleteFileAsync(remoteFilePath));
    
        Assert.That(ex.Message, Is.EqualTo("Error al eliminar el archivo del servidor FTP."));
    }

    [Test]
    public async Task CheckFileExisteFtpException()
    {
        string filePath = "data/nonexistent-file.jpg";

        var mockFtpService = new Mock<FtpService>(_mockLogger.Object, _mockFtpConfig.Object);

        mockFtpService.Setup(s => s.CheckFileExiste(It.IsAny<string>()))
            .ThrowsAsync(new FtpException("Error al verificar existencia de archivo: Error de conexión FTP."));

        _ftpService = mockFtpService.Object;

        var ex = Assert.ThrowsAsync<FtpException>(async () => 
            await _ftpService.CheckFileExiste(filePath));

        Assert.That(ex.Message, Is.EqualTo("Error al verificar existencia de archivo: Error de conexión FTP."));
    }
    
    [Test]
    public async Task ChekDirectorioExisteException()
    {
        var directoryPath = "data/nonexistentdirectory";

        var mockFtpService = new Mock<FtpService>(_mockLogger.Object, _mockFtpConfig.Object);

        mockFtpService.Setup(s => s.ChekDirectorioExiste(It.IsAny<string>()))
            .ThrowsAsync(new WebException("Directorio no disponible"));

        _ftpService = mockFtpService.Object;

        var ex = Assert.ThrowsAsync<WebException>(async () => 
            await _ftpService.ChekDirectorioExiste(directoryPath));

        Assert.That(ex.Message, Is.EqualTo("Directorio no disponible"));
    }
}