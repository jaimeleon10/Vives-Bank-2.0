public class ExportFromZipException : Exception
{
    public ExportFromZipException(string message, Exception innerException) 
        : base(message, innerException) { }
}