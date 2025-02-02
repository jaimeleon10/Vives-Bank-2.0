namespace Banco_VivesBank.Storage.Json.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando un archivo JSON no es encontrado durante una operación de almacenamiento.
/// Esta clase hereda de <see cref="JsonStorageException"/> para proporcionar un tipo específico de error relacionado con el almacenamiento JSON.
/// </summary>
public sealed class JsonNotFoundException : JsonStorageException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="JsonNotFoundException"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    /// <param name="innerException">La excepción interna que causó esta excepción, si la hay.</param>
    public JsonNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

