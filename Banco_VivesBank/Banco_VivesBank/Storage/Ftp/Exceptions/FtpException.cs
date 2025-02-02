namespace Banco_VivesBank.Storage.Ftp.Exceptions;

/// <summary>
/// Excepción personalizada para errores relacionados con FTP.
/// </summary>
public class FtpException : Exception
{
    /// <summary>
    /// Constructor de la excepción FTP.
    /// </summary>
    /// <param name="mensaje">Mensaje de error.</param>
    public FtpException(string mensaje) : base(mensaje) { }
}