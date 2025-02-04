using Banco_VivesBank.Storage.Zip.Exceptions;
using Banco_VivesBank.Storage.Zip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Path = System.IO.Path;

namespace Banco_VivesBank.Storage.Zip.Controller;

/// <summary>
/// Controlador que maneja las operaciones de exportación e importación de archivos ZIP 
/// relacionados con el respaldo de datos en el sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly ILogger<BackupService> _logger;
    private readonly IBackupService _backupService;
    private readonly string _dataDirectory;
    private readonly string _backupFilePath;

    /// <summary>
    /// Constructor del controlador, que inicializa los servicios necesarios y las rutas de archivo.
    /// </summary>
    /// <param name="logger">Instancia de registro de eventos de la aplicación.</param>
    /// <param name="backupService">Instancia del servicio encargado de la exportación e importación de archivos ZIP.</param>
    public BackupController(
        ILogger<BackupService> logger, 
        IBackupService backupService)
    {
        _logger = logger;
        _backupService = backupService;

        _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Backup");
        _backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Backup", "backup.zip");

        Directory.CreateDirectory(_dataDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(_backupFilePath)!);
    }

    /// <summary>
    /// Obtiene la ruta del directorio de datos.
    /// </summary>
    /// <returns>Ruta del directorio de datos.</returns>
    protected virtual string GetDataDirectory() => _dataDirectory;
    /// <summary>
    /// Obtiene la ruta del archivo de respaldo ZIP.
    /// </summary>
    /// <returns>Ruta del archivo de respaldo ZIP.</returns>
    protected virtual string GetBackupFilePath() => _backupFilePath;

    /// <summary>
    /// Exporta los datos del sistema a un archivo ZIP.
    /// Requiere que el usuario tenga el rol de "Admin".
    /// </summary>
    /// <returns>Resultado de la operación HTTP (OK si es exitoso, error en caso contrario).</returns>
    [HttpGet("exportar-zip")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportToZip()
    {
        try
        {
            string sourceDirectory = GetDataDirectory();
            string zipFilePath = GetBackupFilePath();

            if (!Directory.Exists(sourceDirectory))
            {
                _logger.LogWarning($"El directorio fuente no existe: {sourceDirectory}");
                return BadRequest("El directorio fuente no existe.");
            }

            await _backupService.ExportToZip(sourceDirectory, zipFilePath);
            _logger.LogInformation($"Exportación de datos a ZIP completada con éxito. Archivo: {zipFilePath}");

            return Ok();
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argumento nulo en la exportación.");
            return BadRequest("Argumentos de exportación inválidos.");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error al manejar archivos en la exportación.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error de archivo al exportar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar datos a ZIP.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al exportar datos");
        }
    }

    /// <summary>
    /// Importa datos desde un archivo ZIP al sistema.
    /// Requiere que el usuario tenga el rol de "Admin".
    /// </summary>
    /// <returns>Resultado de la operación HTTP (OK si es exitoso, error en caso contrario).</returns>
    [HttpGet("importar-zip")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImportFromZip()
    {
        try
        {
            string zipFilePath = GetBackupFilePath();
            string destinationDirectory = GetDataDirectory();

            if (!System.IO.File.Exists(zipFilePath))
            {
                _logger.LogError($"El archivo ZIP no existe en la ruta: {zipFilePath}");
                return BadRequest("El archivo ZIP no existe.");
            }

            await _backupService.ImportFromZip(zipFilePath, destinationDirectory);
            _logger.LogInformation($"Importación de datos desde ZIP completada con éxito. Archivo: {zipFilePath}");

            return Ok();
        }
        catch (ImportFromZipException ex)
        {
            _logger.LogError(ex, "Error al importar desde ZIP.");
            return BadRequest("Los archivos dentro del ZIP no son válidos o están incompletos.");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error al manejar archivos en la importación.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error de archivo al importar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar datos desde ZIP.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al importar datos");
        }
    }
}
