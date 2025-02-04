using System.Numerics;
using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public class Domiciliacion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("guid")]
    [JsonPropertyName("guid")]
    public string Guid = GuidGenerator.GenerarId();
    
    [BsonElement("clienteGuid")]
    [JsonPropertyName("clienteGuid")]
    public required string ClienteGuid { get; set; }
    
    [BsonElement("acreedor")]
    [JsonPropertyName("acreedor")]
    public required string Acreedor { get; set; }
    
    [BsonElement("ibanEmpresa")]
    [JsonPropertyName("ibanEmpresa")]
    public required string IbanEmpresa { get; set; }
    
    [BsonElement("ibanCliente")]
    [JsonPropertyName("ibanCliente")]
    public required string IbanCliente { get; set; }
    
    [BsonElement("importe")]
    [JsonPropertyName("importe")]
    public required double Importe { get; set; }

    [BsonElement("periodicidad")]
    [JsonPropertyName("periodicidad")]
    public Periodicidad Periodicidad { get; set; } = Periodicidad.Mensual;
    
    [BsonElement("activa")]
    [JsonPropertyName("activa")]
    public bool Activa { get; set; } = true;
    
    [BsonElement("fechaInicio")]
    [JsonPropertyName("fechaInicio")]
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    
    [BsonElement("ultimaEjecucion")]
    [JsonPropertyName("ultimaEjecucion")]
    public DateTime UltimaEjecucion { get; set; } = DateTime.UtcNow;
}