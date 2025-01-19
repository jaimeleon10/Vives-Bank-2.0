namespace Banco_VivesBank.Storage.Backup.Exceptions;

public class ImportFromZipException : Exception
{
    public ImportFromZipException() : base() { }

    public ImportFromZipException(string message) : base(message) { }

    public ImportFromZipException(string message, Exception innerException) 
        : base(message, innerException) { }
}