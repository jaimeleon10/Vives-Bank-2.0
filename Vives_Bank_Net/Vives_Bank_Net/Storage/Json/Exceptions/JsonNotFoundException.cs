namespace Vives_Bank_Net.Storage.Json.Exceptions;

public sealed class JsonNotFoundException : JsonStorageException
{
    public JsonNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

