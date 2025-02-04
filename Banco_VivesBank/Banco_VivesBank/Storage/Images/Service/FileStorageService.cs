using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Config.Storage.Images;
using Microsoft.Extensions.Options;
using Path = System.IO.Path;

namespace Banco_VivesBank.Storage.Images.Service;

/// <summary>
/// Servicio para la gestión del almacenamiento de archivos.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly FileStorageConfig _fileStorageConfig;
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor del servicio de almacenamiento de archivos.
    /// </summary>
    /// <param name="fileStorageConfig">Configuración del almacenamiento de archivos.</param>
    /// <param name="logger">Logger para registrar eventos.</param>
    public FileStorageService(IOptions<FileStorageConfig> fileStorageConfig, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _fileStorageConfig = fileStorageConfig.Value;
    }

    /// <summary>
    /// Guarda un archivo en el almacenamiento.
    /// </summary>
    /// <param name="file">Archivo a almacenar.</param>
    /// <returns>Nombre del archivo guardado.</returns>
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        _logger.LogInformation($"Saving file: {file.FileName}");

        if (file.Length > _fileStorageConfig.MaxFileSize)
            throw new FileStorageException("El tamaño del fichero excede el máximo permitido.");

        var fileExtension = Path.GetExtension(file.FileName);

        var mimeType = MimeTypes.GetMimeType(fileExtension);
        _logger.LogInformation($"Detected MIME type: {mimeType}");

        if (!_fileStorageConfig.AllowedFileTypes.Contains(fileExtension))
            throw new FileStorageException("Tipo de fichero no permitido.");

        var uploadPath = Path.Combine(_fileStorageConfig.UploadDirectory);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var fileName = Guid.NewGuid() + fileExtension;
        var filePath = Path.Combine(uploadPath, fileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        _logger.LogInformation($"File saved: {fileName}");
        return fileName;
    }

    /// <summary>
    /// Obtiene un archivo del almacenamiento.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a recuperar.</param>
    /// <returns>Stream del archivo solicitado.</returns>
    public async Task<FileStream> GetFileAsync(string fileName)
    {
        _logger.LogInformation($"Getting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                throw new FileStorageNotFoundException($"File not found: {fileName}");
            }

            _logger.LogInformation($"File found: {filePath}");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (FileStorageNotFoundException ex)
        {
            _logger.LogError(ex, "File not found");
            throw new FileStorageNotFoundException($"File not found: {fileName}");;
        }
    }

    /// <summary>
    /// Elimina un archivo del almacenamiento.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a eliminar.</param>
    /// <returns>True si el archivo fue eliminado correctamente, false en caso contrario.</returns>
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        _logger.LogInformation($"Deleting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                throw new FileStorageNotFoundException($"File not found: {fileName}");
            }

            File.Delete(filePath);
            _logger.LogInformation($"File deleted: {filePath}");
            return true;
        }
        catch (FileStorageNotFoundException ex)
        {
            _logger.LogError(ex, "File not found during delete");
            throw new FileStorageNotFoundException($"File not found: {fileName}");
        }
    }
}