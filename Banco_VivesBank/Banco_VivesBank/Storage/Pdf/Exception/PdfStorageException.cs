namespace Banco_VivesBank.Storage.Pdf.Exceptions
{
    public class PdfStorageException : Exception
    {
        public PdfStorageException(string message) : base(message) { }

        public PdfStorageException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CuentaInvalidaException : PdfStorageException
    {
        public CuentaInvalidaException(string message) : base(message) { }
    }

    public class MovimientosInvalidosException : PdfStorageException
    {
        public MovimientosInvalidosException(string message) : base(message) { }
    }

    public class PdfGenerateException : PdfStorageException
    {
        public PdfGenerateException(string message) : base(message) { }
    }
}
