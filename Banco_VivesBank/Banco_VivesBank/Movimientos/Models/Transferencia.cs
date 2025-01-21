using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Movimientos.Models;

public class Transferencia
{
    [JsonPropertyName("ibanOrigen")]
    public required string IbanOrigen { get; set; }
    
    [JsonPropertyName("ibanDestino")]
    public required string IbanDestino { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("nombreBeneficiario")]
    public required string NombreBeneficiario { get; set; }
}