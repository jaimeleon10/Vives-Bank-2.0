namespace Banco_VivesBank.Storage.Zip.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando ocurre un error al intentar importar datos desde un archivo ZIP.
/// </summary>
public class ImportFromZipException : Exception
{
    /// <summary>
    /// Constructor para inicializar la excepción con un mensaje de error y una excepción interna.
    /// </summary>
    /// <param name="message">Mensaje de error que describe la razón de la excepción.</param>
    /// <param name="innerException">Excepción interna que causó la excepción actual.</param>
    public ImportFromZipException(string message, Exception innerException) : base(message, innerException) { }
}