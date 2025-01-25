namespace Banco_VivesBank.Storage.Files.Exceptions;

public class FileStorageNotFoundException : FileStorageException
{
    public FileStorageNotFoundException(string message) : base(message)
    {
    }
}