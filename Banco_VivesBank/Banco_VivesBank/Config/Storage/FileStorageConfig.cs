namespace Banco_VivesBank.Config.Storage;

public class FileStorageConfig
{
    public string UploadDirectory { get; set; } = "uploads";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    public List<string> AllowedFileTypes { get; set; } = [".jpeg", ".png", ".jpg"];
    public bool RemoveAll { get; set; } = false;
}