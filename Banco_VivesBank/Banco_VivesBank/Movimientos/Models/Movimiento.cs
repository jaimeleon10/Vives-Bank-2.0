using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public class Movimiento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("guid")]
    [JsonPropertyName("guid")]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [BsonElement("clienteGuid")]
    [JsonPropertyName("clienteGuid")]
    public required string ClienteGuid { get; set; }

    [BsonElement("domiciliacion")]
    [JsonPropertyName("domiciliacion")]
    public Domiciliacion? Domiciliacion { get; set; }

    [BsonElement("ingresoNomina")]
    [JsonPropertyName("ingresoNomina")]
    public IngresoNomina? IngresoNomina { get; set; } 

    [BsonElement("pagoConTarjeta")]
    [JsonPropertyName("pagoConTarjeta")]
    public PagoConTarjeta? PagoConTarjeta { get; set; } 

    [BsonElement("transferencia")]
    [JsonPropertyName("transferencia")]
    public Transferencia? Transferencia { get; set; } 

    [BsonElement("createdAt")]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}