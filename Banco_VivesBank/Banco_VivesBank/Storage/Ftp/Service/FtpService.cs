using Banco_VivesBank.Storage.Ftp.Exceptions;
using Banco_VivesBank.Config.Storage.Images;
using Banco_VivesBank.Config.Storage.Ftp;
using Microsoft.Extensions.Options;
using Path = System.IO.Path;
using System.Net;

namespace Banco_VivesBank.Storage.Ftp.Service;

/// <summary>
/// Servicio para la gestión de archivos en un servidor FTP.
/// </summary>
public class FtpService : IFtpService
{
    private readonly ILogger<FtpService> _logger;
    private readonly FtpConfig _ftpConfig;

    /// <summary>
    /// Constructor del servicio FTP.
    /// </summary>
    /// <param name="logger">Logger para registrar eventos.</param>
    /// <param name="ftpConfig">Configuración del servidor FTP.</param>
    public FtpService(ILogger<FtpService> logger, IOptions<FtpConfig> ftpConfig)
    {
        _ftpConfig = ftpConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// Configura una solicitud FTP.
    /// </summary>
    /// <param name="path">Ruta del archivo en el servidor FTP.</param>
    /// <param name="method">Método FTP a utilizar.</param>
    /// <returns>Objeto FtpWebRequest configurado.</returns>
    public virtual FtpWebRequest ConfigureFtpConsulta(string path, string method)
    {
        var request = (FtpWebRequest)WebRequest.Create(new Uri($"ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{path}"));
        request.Credentials = new NetworkCredential("admin", "password");
        request.Method = method;
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        return request;
    }

    /// <summary>
    /// Sube un archivo al servidor FTP.
    /// </summary>
    /// <param name="inputStream">Stream del archivo a subir.</param>
    /// <param name="uploadPath">Ruta de destino en el servidor FTP.</param>
    public async Task UploadFileAsync(Stream inputStream, string uploadPath)
    {
        try
        {
            var fileExtension = Path.GetExtension(uploadPath);

            var mimeType = MimeTypes.GetMimeType(fileExtension);

            if (mimeType == "application/octet-stream")
            {
                _logger.LogError($"Tipo MIME desconocido para la extensión: {fileExtension}. Operación cancelada.");
                throw new InvalidOperationException($"El tipo MIME para la extensión '{fileExtension}' no es válido.");
            }

            _logger.LogInformation($"Tipo MIME detectado para '{uploadPath}': {mimeType}");
            _logger.LogInformation($"Ruta completa FTP: ftp://{_ftpConfig.Host}:{_ftpConfig.Port}/{uploadPath}");

            await ChekDirectorioExiste("data");

            var request = ConfigureFtpConsulta(uploadPath, WebRequestMethods.Ftp.UploadFile);

            using (var requestStream = request.GetRequestStream())
            {
                await inputStream.CopyToAsync(requestStream);
            }

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Subida completada con estado: {response.StatusDescription}");

                bool exists = await CheckFileExiste(uploadPath);
                _logger.LogInformation($"Archivo existe después de subir: {exists}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error detallado al subir: {ex.GetType()}: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Descarga un archivo desde el servidor FTP.
    /// </summary>
    /// <param name="remoteFilePath">Ruta del archivo en el servidor FTP.</param>
    /// <param name="localFilePath">Ruta local donde se almacenará el archivo.</param>
    public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
    {
        try
        {
            _logger.LogInformation($"Descargando archivo desde el servidor FTP: {remoteFilePath} a {localFilePath}");
            var request = ConfigureFtpConsulta(remoteFilePath, WebRequestMethods.Ftp.DownloadFile);
            
            _logger.LogInformation("Recuperando archivo");
            using (var response = (FtpWebResponse) await request.GetResponseAsync())
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
    
    /// <summary>
    /// Elimina un archivo del servidor FTP.
    /// </summary>
    /// <param name="remoteFilePath">Ruta del archivo en el servidor FTP.</param>
    public async Task DeleteFileAsync(string remoteFilePath)
    {
        try
        {
            var request = ConfigureFtpConsulta(remoteFilePath, WebRequestMethods.Ftp.DeleteFile);

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
    
    /// <summary>
    /// Crea un directorio en el servidor FTP.
    /// </summary>
    /// <param name="directoryPath">Ruta del directorio en el servidor FTP.</param>
    public virtual async Task ChekDirectorioExiste(string directoryPath)
    {
        try
        {
            var request = ConfigureFtpConsulta(directoryPath, WebRequestMethods.Ftp.MakeDirectory);
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"Directorio creado/verificado: {response.StatusDescription}");
            }
        }
        catch (WebException ex)
        {
            var response = (FtpWebResponse)ex.Response;
            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                _logger.LogInformation($"Directorio ya existe: {directoryPath}");
            }
            else
            {
                _logger.LogError($"Error creando directorio: {ex.Message}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Verifica si un archivo existe en el servidor FTP.
    /// </summary>
    /// <param name="filePath">Ruta del archivo en el servidor FTP.</param>
    /// <returns>True si el archivo existe, False en caso contrario.</returns>
    public virtual async Task<bool> CheckFileExiste(string filePath)
    {
        try
        {
            var request = ConfigureFtpConsulta(filePath, WebRequestMethods.Ftp.GetFileSize);
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.LogInformation($"El archivo existe con un tamaño de: {response.ContentLength} bytes");
                return true;
            }
        }
        catch (WebException ex)
        {
            var response = ex.Response as FtpWebResponse;
            if (response?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                _logger.LogWarning($"El archivo no existe: {filePath}");
                return false;
            }
            throw new FtpException("Error al verificar existencia de archivo: " + ex.Message);
        }
    }
}