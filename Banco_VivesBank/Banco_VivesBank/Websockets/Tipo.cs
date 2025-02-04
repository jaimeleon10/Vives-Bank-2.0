using System.Text.Json.Serialization;

namespace Banco_VivesBank.Websockets;

/// <summary>
/// Enumeración que representa los tipos de acciones que pueden realizarse en una operación de WebSocket.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Tipo
{
    /// <summary>
    /// Acción para crear un nuevo recurso.
    /// </summary>
    CREATE,

    /// <summary>
    /// Acción para actualizar un recurso existente.
    /// </summary>
    UPDATE,

    /// <summary>
    /// Acción para eliminar un recurso existente.
    /// </summary>
    DELETE
}
