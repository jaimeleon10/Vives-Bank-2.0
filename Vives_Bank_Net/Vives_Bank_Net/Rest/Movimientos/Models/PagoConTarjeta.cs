using System.Numerics;
using System.Text.Json.Serialization;

namespace Vives_Bank_Net.Rest.Movimientos.Models;

public abstract class PagoConTarjeta
{
    [JsonPropertyName("numeroTarjeta")]
    public required string NumeroTarjeta { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("nombreComercio")]
    public required string NombreComercio { get; set; }
}