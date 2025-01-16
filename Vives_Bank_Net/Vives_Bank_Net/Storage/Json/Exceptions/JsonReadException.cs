namespace Vives_Bank_Net.Storage.Json.Exceptions;

public sealed class JsonReadException : JsonStorageException
{
    public JsonReadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}