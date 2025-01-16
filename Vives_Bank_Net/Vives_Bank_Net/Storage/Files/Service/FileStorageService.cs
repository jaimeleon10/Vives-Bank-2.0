using Microsoft.Extensions.Options;
using Vives_Bank_Net.Config.Storage;
using Vives_Bank_Net.Storage.Exceptions;
using Exception = System.Exception;

namespace Vives_Bank_Net.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageConfig _fileStorageConfig;
    private readonly ILogger _logger;

    public FileStorageService(IOptions<FileStorageConfig> fileStorageConfig, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _fileStorageConfig = fileStorageConfig.Value;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        _logger.LogInformation($"Saving file: {file.FileName}");

        if (file.Length > _fileStorageConfig.MaxFileSize)
            throw new FileStorageException("El tamaño del fichero excede el máximo permitido.");

        var fileExtension = Path.GetExtension(file.FileName);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file");
            throw new FileStorageException($"Error getting file: {fileName}");
        }
    }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            throw new FileStorageException($"Error deleting file: {fileName}");
        }
    }
}