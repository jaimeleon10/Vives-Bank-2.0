using Banco_VivesBank.Storage.Ftp.Service;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Storage.Ftp;

[ApiController]
[Route("[controller]")]
public class FtpController : ControllerBase
{
    private readonly IFtpService _ftpService;
    private readonly ILogger<FtpController> _logger;

    public FtpController(IFtpService ftpService, ILogger<FtpController> logger)
    {
        _ftpService = ftpService;
        _logger = logger;
    }

    [HttpPost("subir-archivo")]
    public async Task<IActionResult> SubirArchivo(IFormFile archivo, [FromQuery] string rutaSubida)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo vacío.");

            using (var stream = archivo.OpenReadStream())
            {
                await _ftpService.UploadFileAsync(stream, rutaSubida);
            }

            return Ok("Archivo subido exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fallo al cargar el archivo: {ex.Message}");
            return BadRequest($"Fallo al cargar el archivo: {ex.Message}");        }
    }

    [HttpPost("descargar-archivo")]
    public async Task<IActionResult> DescargarArchivo([FromQuery] string rutaArchivoRemoto, [FromQuery] string rutaLocal)
    {
        try
        {
            await _ftpService.DownloadFileAsync(rutaArchivoRemoto, rutaLocal);
            return Ok($"Archivo descargado exitosamente en: {rutaLocal}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fallo al descargar el archivo: {ex.Message}");
            return BadRequest($"Fallo al descargar el archivo: {ex.Message}");        }
    }

    [HttpDelete("eliminar-archivo")]
    public async Task<IActionResult> EliminarArchivo([FromQuery] string rutaArchivoRemoto)
    {
        try
        {
            await _ftpService.DeleteFileAsync(rutaArchivoRemoto);
            return Ok("Archivo eliminado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fallo al eliminar el archivo: {ex.Message}");
            return BadRequest($"Fallo al eliminar el archivo: {ex.Message}");        }
    }
}
