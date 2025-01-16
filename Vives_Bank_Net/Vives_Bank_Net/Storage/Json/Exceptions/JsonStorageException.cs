namespace Vives_Bank_Net.Storage.Json.Exceptions;

public class JsonStorageException : Exception
{
    public JsonStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}