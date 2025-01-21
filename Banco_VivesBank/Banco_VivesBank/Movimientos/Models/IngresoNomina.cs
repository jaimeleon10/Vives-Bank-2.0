using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Movimientos.Models;

public class IngresoNomina
{
    [JsonPropertyName("ibanOrigen")]
    public required string IbanOrigen { get; set; }
    
    [JsonPropertyName("ibanDestino")]
    public required string IbanDestino { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("nombreEmpresa")]
    public required string NombreEmpresa { get; set; }
    
    [JsonPropertyName("cifEmpresa")]
    public required string CifEmpresa { get; set; }
}