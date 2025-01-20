namespace Banco_VivesBank.Storage.Backup.Exceptions;

public class ImportFromZipException : Exception
{
    public ImportFromZipException(string message, Exception innerException) 
        : base(message, innerException) { }
}