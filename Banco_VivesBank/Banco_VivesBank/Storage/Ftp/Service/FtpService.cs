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

    public string GetFtpHost()
    {
        return $"{_ftpConfig.Host}:{_ftpConfig.Port}";
    }

    public async Task UploadFileAsync(Stream inputStream, string uploadPath)
    {
        try
        {
            _logger.LogInformation($"Intentando subir archivo a la ruta: {uploadPath}");
            var request = ConfigureFtpRequest(uploadPath, WebRequestMethods.Ftp.UploadFile);
            
            _logger.LogInformation(WebRequestMethods.Ftp.UploadFile);

            _logger.LogInformation($"URI de la solicitud FTP: {request.RequestUri}");
            using (var requestStream = request.GetRequestStream())
            {
                await inputStream.CopyToAsync(requestStream);
            }

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Subida completada con estado: {response.StatusDescription}");
            }

            // Verificar si el archivo se subió correctamente
            bool fileExists = await FileExistsAsync(uploadPath);
            if (!fileExists)
            {
                _logger.LogError("El archivo no existe en el servidor después de la subida.");
                throw new FtpException("El archivo no se encuentra en el servidor FTP.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al subir el archivo al servidor FTP: {ex.Message}");
            throw new FtpException("Error al cargar el archivo al servidor FTP.");
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var request = ConfigureFtpRequest(filePath, WebRequestMethods.Ftp.GetFileSize);
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"El archivo existe con un tamaño de: {response.ContentLength} bytes");
                return true;
            }
        }
        catch (WebException ex)
        {
            _logger.LogWarning($"El archivo no existe: {ex.Message}");
            return false;
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

                _logger.LogInformation($"Descarga completada con estado: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar el archivo desde el servidor FTP: {ex.Message}");
            throw new FtpException("Error al descargar el archivo desde el servidor FTP.");
        }
    }

    public async Task DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            var request = ConfigureFtpRequest(remoteFilePath, WebRequestMethods.Ftp.DeleteFile);

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Eliminación completada con estado: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar el archivo del servidor FTP: {ex.Message}");
            throw new FtpException("Error al eliminar el archivo del servidor FTP.");
        }
    }
}