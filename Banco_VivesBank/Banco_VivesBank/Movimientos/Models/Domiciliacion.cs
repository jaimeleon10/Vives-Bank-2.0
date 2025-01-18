using System.Numerics;
using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Banco_VivesBank.Movimientos.Models;

public abstract class Domiciliacion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [JsonPropertyName("guid")]
    public string Guid = GuidGenerator.GenerarId();
    
    [JsonPropertyName("cliente")]
    public required Cliente.Models.Cliente Cliente { get; set; }
    
    [JsonPropertyName("ibanOrigen")]
    public required string IbanOrigen { get; set; }
    
    [JsonPropertyName("ibanDestino")]
    public required string IbanDestino { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("acreedor")]
    public required string Acreedor { get; set; }
    
    [JsonPropertyName("fechaInicio")]
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("periodicidad")]
    public Periodicidad Periodicidad { get; set; } = Periodicidad.Mensual;
    
    [JsonPropertyName("activa")]
    public bool Activa { get; set; } = true;
    
    [JsonPropertyName("ultimaEjecucion")]
    public DateTime UltimaEjecucion { get; set; } = DateTime.UtcNow;
}