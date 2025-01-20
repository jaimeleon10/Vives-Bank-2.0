using Microsoft.AspNetCore.Mvc;

[ApiController]
public class BackupController
{
    private readonly ILogger<BackupService> _logger;
    private readonly IBackupService _backupService;

    public BackupController(ILogger<BackupService> logger, IBackupService backupService)
    {
        _logger = logger;
        _backupService = backupService;
    }

    private string GetDataDirectory()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    }

    private string GetBackupFilePath()
    {
        return Path.Combine(GetDataDirectory(), "backup.zip");
    }

    [HttpGet("exportar-zip")]
    public async Task ExportToZip()
    {
        try
        {
            string sourceDirectory = GetDataDirectory();
            string zipFilePath = GetBackupFilePath();

            await _backupService.ExportToZip(sourceDirectory, zipFilePath);
            _logger.LogInformation($"Exportación de datos a ZIP completada con éxito. Archivo: {zipFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar datos a ZIP.");
            throw;
        }
    }

    [HttpGet("importar-zip")]
    public async Task ImportFromZip()
    {
        try
        {
            string zipFilePath = GetBackupFilePath();
            string destinationDirectory = GetDataDirectory();

            await _backupService.ImportFromZip(zipFilePath, destinationDirectory);
            _logger.LogInformation($"Importación de datos desde ZIP completada con éxito. Archivo: {zipFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar datos desde ZIP.");
            throw;
        }
    }
}
