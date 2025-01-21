public interface IBackupService
{
    Task ImportFromZip(string zipFilePath, string destinationDirectory);
    
    Task ExportToZip(string sourceDirectory, string zipFilePath);
}