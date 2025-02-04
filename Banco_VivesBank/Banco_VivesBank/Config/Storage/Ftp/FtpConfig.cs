namespace Banco_VivesBank.Config.Storage.Ftp;

public class FtpConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 21;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "password";
}
