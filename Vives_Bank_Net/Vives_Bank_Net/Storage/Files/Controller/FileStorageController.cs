namespace Vives_Bank_Net.Storage.Files.Controller;
/*
[ApiController]
[Route("api/[controller]")]
public class FileStorageController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileStorageController> _logger;

    public FileStorageController(IFileStorageService fileStorageService, ILogger<FileStorageController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        try
        {
            var savedFileName = await _fileStorageService.SaveFileAsync(file);
            return Ok(new { FileName = savedFileName });
        }
        catch (FileStorageException ex)
        {
            _logger.LogWarning(ex, "Error uploading file");
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file");
            return StatusCode(500, "An error occurred while uploading the file.");
        }
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        try
        {
            var fileStream = await _fileStorageService.GetFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (FileStorageException ex)
        {
            _logger.LogWarning(ex, "Error downloading file");
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading file");
            return StatusCode(500, "An error occurred while downloading the file.");
        }
    }

    [HttpDelete("delete/{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        try
        {
            var success = await _fileStorageService.DeleteFileAsync(fileName);
            if (!success)
            {
                return NotFound(new { Error = "File not found." });
            }

            return Ok(new { Message = "File deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting file");
            return StatusCode(500, "An error occurred while deleting the file.");
        }
    }
}*/