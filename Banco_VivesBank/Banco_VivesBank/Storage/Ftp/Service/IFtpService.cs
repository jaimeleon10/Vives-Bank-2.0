namespace Banco_VivesBank.Storage.Ftp.Service;

/// <summary>
/// Interfaz para la gestión de archivos en un servidor FTP.
/// </summary>
public interface IFtpService
{
    /// <summary>
    /// Sube un archivo al servidor FTP.
    /// </summary>
    /// <param name="inputStream">Stream del archivo a subir.</param>
    /// <param name="uploadPath">Ruta de destino en el servidor FTP.</param>
    public Task UploadFileAsync(Stream inputStream, string uploadPath);
    
    /// <summary>
    /// Descarga un archivo desde el servidor FTP.
    /// </summary>
    /// <param name="remoteFilePath">Ruta del archivo en el servidor FTP.</param>
    /// <param name="localFilePath">Ruta local donde se almacenará el archivo.</param>
    public Task DownloadFileAsync(string remoteFilePath, string localFilePath);
    
    /// <summary>
    /// Elimina un archivo del servidor FTP.
    /// </summary>
    /// <param name="remoteFilePath">Ruta del archivo en el servidor FTP.</param>
    public Task DeleteFileAsync(string remoteFilePath);
    
    /// <summary>
    /// Verifica si un archivo existe en el servidor FTP.
    /// </summary>
    /// <param name="remotePath">Ruta del archivo en el servidor FTP.</param>
    /// <returns>Devuelve true si el archivo existe, false en caso contrario.</returns>
    Task<bool> CheckFileExiste(string remotePath);
}