namespace Banco_VivesBank.Storage.Ftp.Exceptions;

public class FtpException : Exception
{
    public FtpException(string mensaje) : base(mensaje) { }
}
