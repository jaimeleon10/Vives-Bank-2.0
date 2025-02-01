using System.Text.Json.Serialization;

namespace Banco_VivesBank.Websockets;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Tipo
{
    CREATE,
    UPDATE,
    DELETE
}