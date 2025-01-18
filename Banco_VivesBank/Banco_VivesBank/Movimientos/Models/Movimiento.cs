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

    [JsonPropertyName("guid")]
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [JsonPropertyName("cliente")]
    public required Cliente.Models.Cliente Cliente { get; set; }

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}