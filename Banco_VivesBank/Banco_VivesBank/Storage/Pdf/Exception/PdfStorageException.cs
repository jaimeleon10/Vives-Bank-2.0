namespace Banco_VivesBank.Storage.Pdf.Exception;

public class PdfStorageException : System.Exception
{
    public PdfStorageException(string message) : base(message) { }
}

public class MovimientosInvalidosException : PdfStorageException
{
    public MovimientosInvalidosException(string message) : base(message) { }
}

public class PdfGenerateException : PdfStorageException
{
    public PdfGenerateException(string message) : base(message) { }
}
