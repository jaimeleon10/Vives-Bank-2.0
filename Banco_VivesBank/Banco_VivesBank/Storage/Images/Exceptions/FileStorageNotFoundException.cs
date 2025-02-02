namespace Banco_VivesBank.Storage.Images.Exceptions;

/// <summary>
/// Excepción lanzada cuando no se encuentra un archivo en el almacenamiento.
/// </summary>
public class FileStorageNotFoundException : FileStorageException
{
    /// <summary>
    /// Constructor de la excepción cuando un archivo no se encuentra.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    public FileStorageNotFoundException(string message) : base(message) { }
}