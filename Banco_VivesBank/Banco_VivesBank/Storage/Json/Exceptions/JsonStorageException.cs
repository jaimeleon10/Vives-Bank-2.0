namespace Banco_VivesBank.Storage.Json.Exceptions;

/// <summary>
/// Excepción base personalizada para errores relacionados con el almacenamiento de archivos JSON.
/// Esta clase hereda de <see cref="Exception"/> y sirve como base para excepciones más específicas,
/// como <see cref="JsonReadException"/> y <see cref="JsonNotFoundException"/>.
/// </summary>
public class JsonStorageException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="JsonStorageException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    /// <param name="innerException">La excepción interna que causó esta excepción, si la hay.</param>
    public JsonStorageException(string message, Exception innerException) : base(message, innerException) { }
}