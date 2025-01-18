namespace Banco_VivesBank.Storage.Json.Exceptions;

public class JsonStorageException : Exception
{
    public JsonStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}