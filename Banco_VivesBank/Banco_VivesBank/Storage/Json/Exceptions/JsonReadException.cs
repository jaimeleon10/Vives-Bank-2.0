namespace Banco_VivesBank.Storage.Json.Exceptions;

public sealed class JsonReadException : JsonStorageException
{
    public JsonReadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}