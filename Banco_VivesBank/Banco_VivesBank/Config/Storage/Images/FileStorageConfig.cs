namespace Banco_VivesBank.Config.Storage.Images;

public class FileStorageConfig
{
    public string UploadDirectory { get; set; } = "data/avatares";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    public List<string> AllowedFileTypes { get; set; } = [".jpeg", ".png", ".jpg"];
    public bool RemoveAll { get; set; } = false;
}