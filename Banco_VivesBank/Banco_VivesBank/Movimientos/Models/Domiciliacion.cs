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
    
    [JsonPropertyName("guid")]
    public string Guid = GuidGenerator.GenerarId();
    
    [JsonPropertyName("cliente")]
    public required string ClienteGuid { get; set; }
    
    [JsonPropertyName("acreedor")]
    public required string Acreedor { get; set; }
    
    [JsonPropertyName("ibanEmpresa")]
    public required string IbanEmpresa { get; set; }
    
    [JsonPropertyName("ibanCliente")]
    public required string IbanCliente { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }

    [JsonPropertyName("periodicidad")]
    public Periodicidad Periodicidad { get; set; } = Periodicidad.Mensual;
    
    [JsonPropertyName("activa")]
    public bool Activa { get; set; } = true;
    
    [JsonPropertyName("fechaInicio")]
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("ultimaEjecucion")]
    public DateTime UltimaEjecucion { get; set; } = DateTime.UtcNow;
}