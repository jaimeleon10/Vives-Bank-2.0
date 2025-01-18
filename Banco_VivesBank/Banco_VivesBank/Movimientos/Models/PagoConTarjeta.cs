using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Movimientos.Models;

public abstract class PagoConTarjeta
{
    [JsonPropertyName("numeroTarjeta")]
    public required string NumeroTarjeta { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("nombreComercio")]
    public required string NombreComercio { get; set; }
}