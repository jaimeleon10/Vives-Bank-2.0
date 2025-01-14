using System.Text.Json.Serialization;

namespace Vives_Bank_Net.Rest.Movimientos.Models;

public abstract class Transferencia
{
    [JsonPropertyName("ibanOrigen")]
    public required string IbanOrigen { get; set; }
    
    [JsonPropertyName("ibanDestino")]
    public required string IbanDestino { get; set; }
    
    [JsonPropertyName("importe")]
    public required decimal Importe { get; set; }
    
    [JsonPropertyName("nombreBeneficiario")]
    public required string NombreBeneficiario { get; set; }
}