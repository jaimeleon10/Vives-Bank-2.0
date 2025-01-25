namespace Banco_VivesBank.Storage.Zip.Exceptions;

public class ImportFromZipException : Exception
{
    public ImportFromZipException(string message, Exception innerException) : base(message, innerException) { }
}