/*using Banco_VivesBank.Config.Storage;
using Banco_VivesBank.Storage.Ftp.Exceptions;
using Banco_VivesBank.Storage.Ftp.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Storage.Ftp;

[TestFixture]
public class FtpServiceTests
{
    private Mock<ILogger<FtpService>> _loggerMock;
    private Mock<IOptions<FtpConfig>> _ftpConfigMock;
    private FtpService _ftpService;
    private Mock<Stream> _streamMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<FtpService>>();
        _ftpConfigMock = new Mock<IOptions<FtpConfig>>();
        _ftpConfigMock.Setup(c => c.Value).Returns(new FtpConfig
        {
            Host = "ftp.test.com",
            Port = 21
        });

        _ftpService = new FtpService(_loggerMock.Object, _ftpConfigMock.Object);
        _streamMock = new Mock<Stream>();
    }

    [Test]
    public async Task UploadFileAsync_ShouldUploadSuccessfully()
    {
        var uploadPath = "path/to/file.txt";

        await _ftpService.UploadFileAsync(_streamMock.Object, uploadPath);

        _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void UploadFileAsync_ShouldThrowFtpException_WhenErrorOccurs()
    {
        var uploadPath = "path/to/file.txt";
        _ftpConfigMock.Setup(c => c.Value.Host).Throws(new Exception("FTP Error"));

        Assert.ThrowsAsync<FtpException>(() => _ftpService.UploadFileAsync(_streamMock.Object, uploadPath));
    }
    
    [Test]
    public async Task DownloadFileAsync_ShouldDownloadSuccessfully()
    {
        var remoteFilePath = "path/to/remote/file.txt";
        var localFilePath = "path/to/local/file.txt";

        await _ftpService.DownloadFileAsync(remoteFilePath, localFilePath);

        _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void DownloadFileAsync_ShouldThrowFtpException_WhenErrorOccurs()
    {
        var remoteFilePath = "path/to/remote/file.txt";
        var localFilePath = "path/to/local/file.txt";
        _ftpConfigMock.Setup(c => c.Value.Host).Throws(new Exception("FTP Error"));

        Assert.ThrowsAsync<FtpException>(() => _ftpService.DownloadFileAsync(remoteFilePath, localFilePath));
    }
    
    [Test]
    public async Task DeleteFileAsync_ShouldDeleteSuccessfully()
    {
        var remoteFilePath = "path/to/remote/file.txt";

        await _ftpService.DeleteFileAsync(remoteFilePath);

        _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void DeleteFileAsync_ShouldThrowFtpException_WhenErrorOccurs()
    {
        var remoteFilePath = "path/to/remote/file.txt";
        _ftpConfigMock.Setup(c => c.Value.Host).Throws(new Exception("FTP Error"));

        Assert.ThrowsAsync<FtpException>(() => _ftpService.DeleteFileAsync(remoteFilePath));
    }
}*/