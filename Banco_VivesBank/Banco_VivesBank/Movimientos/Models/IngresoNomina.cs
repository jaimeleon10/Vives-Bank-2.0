using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Movimientos.Models;

public class IngresoNomina
{
    [JsonPropertyName("nombreEmpresa")]
    public required string NombreEmpresa { get; set; }
    
    [JsonPropertyName("cifEmpresa")]
    public required string CifEmpresa { get; set; }
    
    [JsonPropertyName("ibanOrigen")]
    public required string IbanEmpresa { get; set; }
    
    [JsonPropertyName("ibanDestino")]
    public required string IbanCliente { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
}