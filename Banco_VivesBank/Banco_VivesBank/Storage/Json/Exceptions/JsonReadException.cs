namespace Banco_VivesBank.Storage.Json.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando ocurre un error al leer un archivo JSON durante una operación de almacenamiento.
/// Esta clase hereda de <see cref="JsonStorageException"/> para proporcionar un tipo específico de error relacionado con la lectura de archivos JSON.
/// </summary>
public sealed class JsonReadException : JsonStorageException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="JsonReadException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    /// <param name="innerException">La excepción interna que causó esta excepción, si la hay.</param>
    public JsonReadException(string message, Exception innerException) : base(message, innerException) { }
}