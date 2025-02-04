namespace Banco_VivesBank.Storage.Zip.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando ocurre un error al intentar exportar datos desde un archivo ZIP.
/// </summary>
public class ExportFromZipException : Exception
{
    /// <summary>
    /// Constructor para inicializar la excepción con un mensaje de error y una excepción interna.
    /// </summary>
    /// <param name="message">Mensaje de error que describe la razón de la excepción.</param>
    /// <param name="innerException">Excepción interna que causó la excepción actual.</param>
    public ExportFromZipException(string message, Exception innerException) 
        : base(message, innerException) { }
}