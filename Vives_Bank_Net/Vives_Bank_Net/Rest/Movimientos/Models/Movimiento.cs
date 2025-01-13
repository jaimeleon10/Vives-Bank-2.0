using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Movimientos.Models;

public class Movimiento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [JsonPropertyName("guid")]
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [JsonPropertyName("clienteId")]
    public required string ClienteId { get; set; }

    [JsonPropertyName("domiciliacion")]
    public Domiciliacion? Domiciliacion { get; set; }

    [JsonPropertyName("ingresoNomina")]
    public IngresoNomina? IngresoNomina { get; set; } 

    [JsonPropertyName("pagoConTarjeta")]
    public PagoConTarjeta? PagoConTarjeta { get; set; } 

    [JsonPropertyName("transferencia")]
    public Transferencia? Transferencia { get; set; } 

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}