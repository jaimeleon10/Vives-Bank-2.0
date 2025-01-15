namespace Vives_Bank_Net.Storage.Exceptions;

public sealed class JsonReadException : JsonStorageException
{
    public JsonReadException(string message) : base(message)
    {
    }

    public JsonReadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}