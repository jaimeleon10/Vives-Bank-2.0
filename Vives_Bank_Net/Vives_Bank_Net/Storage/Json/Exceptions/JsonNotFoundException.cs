namespace Vives_Bank_Net.Storage.Exceptions;

public sealed class JsonNotFoundException : JsonStorageException
{
    public JsonNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

