using System.Numerics;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public class IngresoNomina
{
    [BsonElement("nombreEmpresa")]
    [JsonPropertyName("nombreEmpresa")]
    public required string NombreEmpresa { get; set; }
    
    [BsonElement("cifEmpresa")]
    [JsonPropertyName("cifEmpresa")]
    public required string CifEmpresa { get; set; }
    
    [BsonElement("ibanEmpresa")]
    [JsonPropertyName("ibanEmpresa")]
    public required string IbanEmpresa { get; set; }
    
    [BsonElement("ibanCliente")]
    [JsonPropertyName("ibanCliente")]
    public required string IbanCliente { get; set; }
    
    [BsonElement("importe")]
    [JsonPropertyName("importe")]
    public required double Importe { get; set; }
}