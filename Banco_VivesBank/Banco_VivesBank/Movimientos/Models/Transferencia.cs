using System.Numerics;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public class Transferencia
{
    [BsonElement("clienteOrigen")]
    [JsonPropertyName("clienteOrigen")]
    public required string ClienteOrigen { get; set; }
    
    [BsonElement("ibanOrigen")]
    [JsonPropertyName("ibanOrigen")]
    public required string IbanOrigen { get; set; }
    
    [BsonElement("nombreBeneficiario")]
    [JsonPropertyName("nombreBeneficiario")]
    public required string NombreBeneficiario { get; set; }
    
    [BsonElement("ibanDestino")]
    [JsonPropertyName("ibanDestino")]
    public required string IbanDestino { get; set; }
    
    [BsonElement("importe")]
    [JsonPropertyName("importe")]
    public required double Importe { get; set; }

    [BsonElement("revocada")]
    [JsonPropertyName("revocada")]
    public bool Revocada { get; set; } = false;
}