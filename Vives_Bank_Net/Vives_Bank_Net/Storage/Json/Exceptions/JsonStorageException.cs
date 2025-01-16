namespace Vives_Bank_Net.Storage.Exceptions;

public class JsonStorageException : Exception
{
    public JsonStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}