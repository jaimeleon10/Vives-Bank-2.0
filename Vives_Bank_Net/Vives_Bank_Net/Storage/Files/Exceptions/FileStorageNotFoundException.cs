namespace Vives_Bank_Net.Storage.Exceptions;

public class FileStorageNotFoundException : FileStorageException
{
    public FileStorageNotFoundException(string message) : base(message)
    {
    }
}