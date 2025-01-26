namespace Banco_VivesBank.Storage.Ftp.Service;

public interface IFtpService
{
    public Task UploadFileAsync(Stream inputStream, string uploadPath);
    public Task DownloadFileAsync(string remoteFilePath, string localFilePath);
    public Task DeleteFileAsync(string remoteFilePath);
    string GetFtpHost();
}