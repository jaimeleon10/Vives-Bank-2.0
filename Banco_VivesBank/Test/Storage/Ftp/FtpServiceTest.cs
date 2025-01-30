using System.Text;
using Banco_VivesBank.Config.Storage;
using Banco_VivesBank.Config.Storage.Ftp;
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

        // Configura el mock para simular el comportamiento deseado
        mockFtpService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mockFtpService.Setup(s => s.CheckFileExiste(It.IsAny<string>()))
            .ReturnsAsync(true); // Simula que el archivo existe
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
}