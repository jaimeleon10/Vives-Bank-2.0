using Banco_VivesBank.Storage.Backup.Service;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Storage.Backup.Controller;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly ILogger<BackupService> _logger;
    private readonly IBackupService _backupService;

    public BackupController(ILogger<BackupService> logger, IBackupService backupService)
    {
        _logger = logger;
        _backupService = backupService;
    }

    protected virtual string GetDataDirectory() => 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

    protected virtual string GetBackupFilePath() => 
        Path.Combine(GetDataDirectory(), "backup.zip");

    [HttpGet("exportar-zip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportToZip()
    {
        try
        {
            string sourceDirectory = GetDataDirectory();
            string zipFilePath = GetBackupFilePath();

            await _backupService.ExportToZip(sourceDirectory, zipFilePath);
            _logger.LogInformation($"Exportación de datos a ZIP completada con éxito. Archivo: {zipFilePath}");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar datos a ZIP.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al exportar datos");
        }
    }

    [HttpGet("importar-zip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImportFromZip()
    {
        try
        {
            string zipFilePath = GetBackupFilePath();
            string destinationDirectory = GetDataDirectory();

            await _backupService.ImportFromZip(zipFilePath, destinationDirectory);
            _logger.LogInformation($"Importación de datos desde ZIP completada con éxito. Archivo: {zipFilePath}");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar datos desde ZIP.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al importar datos");
        }
    }
}
