namespace Banco_VivesBank.Storage.Pdf.Exception;

/// <summary>
/// Excepción base personalizada para errores relacionados con el almacenamiento de archivos PDF.
/// Esta clase hereda de <see cref="System.Exception"/> y sirve como clase base para excepciones más específicas,
/// como <see cref="MovimientosInvalidosException"/> y <see cref="PdfGenerateException"/>.
/// </summary>
public class PdfStorageException : System.Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PdfStorageException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    public PdfStorageException(string message) : base(message) { }
}

/// <summary>
/// Excepción lanzada cuando los movimientos proporcionados para el archivo PDF no son válidos.
/// Hereda de <see cref="PdfStorageException"/> para manejar errores relacionados con movimientos inválidos en la generación o almacenamiento de PDFs.
/// </summary>
public class MovimientosInvalidosException : PdfStorageException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="MovimientosInvalidosException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    public MovimientosInvalidosException(string message) : base(message) { }
}

/// <summary>
/// Excepción lanzada cuando ocurre un error en la generación de un archivo PDF.
/// Hereda de <see cref="PdfStorageException"/> para manejar errores específicos en la generación de PDFs.
/// </summary>
public class PdfGenerateException : PdfStorageException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PdfGenerateException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    public PdfGenerateException(string message) : base(message) { }
}
