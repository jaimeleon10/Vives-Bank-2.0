namespace Banco_VivesBank.Storage.Images.Exceptions;

/// <summary>
/// Excepción personalizada para errores en el almacenamiento de archivos.
/// </summary>
public class FileStorageException : Exception
{
    /// <summary>
    /// Constructor de la excepción de almacenamiento de archivos.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    public FileStorageException(string message) : base(message) { }
}