using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Movimientos.Models;

public class PagoConTarjeta
{
    [JsonPropertyName("nombreComercio")]
    public required string NombreComercio { get; set; }
    
    [JsonPropertyName("importe")]
    public required BigInteger Importe { get; set; }
    
    [JsonPropertyName("numeroTarjeta")]
    public required string NumeroTarjeta { get; set; }
}