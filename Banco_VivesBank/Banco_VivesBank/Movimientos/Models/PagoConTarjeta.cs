using System.Numerics;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public class PagoConTarjeta
{
    [BsonElement("nombreComercio")]
    [JsonPropertyName("nombreComercio")]
    public required string NombreComercio { get; set; }
    
    [BsonElement("importe")]
    [JsonPropertyName("importe")]
    public required double Importe { get; set; }
    
    [BsonElement("numeroTarjeta")]
    [JsonPropertyName("numeroTarjeta")]
    public required string NumeroTarjeta { get; set; }
}