namespace Banco_VivesBank.Storage.Images.Exceptions;

public class FileStorageNotFoundException : FileStorageException
{
    public FileStorageNotFoundException(string message) : base(message)
    {
    }
}