using System.Net;
using Banco_VivesBank.Config.Storage;
using Banco_VivesBank.Storage.Ftp.Exceptions;
using Banco_VivesBank.Storage.Ftp.Service;
using Microsoft.Extensions.Options;

namespace Banco_VivesBank.Storage.Ftp.Service;

public class FtpService : IFtpService
{
    private readonly ILogger<FtpService> _logger;
    private readonly FtpConfig _ftpConfig;

    public FtpService(ILogger<FtpService> logger, IOptions<FtpConfig> ftpConfig)
    {
        _ftpConfig = ftpConfig.Value;
        _logger = logger;
    }

    public virtual FtpWebRequest ConfigureFtpRequest(string path, string method)
    {
        var request = (FtpWebRequest)WebRequest.Create(new Uri($"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{path}"));
        request.Credentials = new NetworkCredential("admin", "password");
        request.Method = method;
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        return request;
    }

    public async Task UploadFileAsync(Stream inputStream, string uploadPath)
    {
        try
        {
            var request = ConfigureFtpRequest(uploadPath, WebRequestMethods.Ftp.UploadFile);
            using (var requestStream = request.GetRequestStream())
            {
                await inputStream.CopyToAsync(requestStream);
            }

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Upload completed with status: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading file to FTP server: {ex.Message}");
            throw new FtpException("Error al cargar el archivo al servidor FTP.");
        }
    }

    public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
    {
        try
        {
            var request = ConfigureFtpRequest(remoteFilePath, WebRequestMethods.Ftp.DownloadFile);

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var fileStream = new FileStream(localFilePath, FileMode.Create))
            {
                if (responseStream != null)
                    await responseStream.CopyToAsync(fileStream);

                _logger.LogInformation($"Download completed with status: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error downloading file from FTP server: {ex.Message}");
            throw new FtpException("Error al cargar el archivo al servidor FTP.");
        }
    }

    public async Task DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            var request = ConfigureFtpRequest(remoteFilePath, WebRequestMethods.Ftp.DeleteFile);

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Delete completed with status: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting file from FTP server: {ex.Message}");
            throw new FtpException("Error al cargar el archivo al servidor FTP.");
        }
    }
}