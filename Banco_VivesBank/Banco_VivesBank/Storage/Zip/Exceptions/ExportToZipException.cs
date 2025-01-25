namespace Banco_VivesBank.Storage.Zip.Exceptions;

public class ExportFromZipException : Exception
{
    public ExportFromZipException(string message, Exception innerException) 
        : base(message, innerException) { }
}