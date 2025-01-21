using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class PagoConTarjetaResponse
{
    public string NumeroTarjeta { get; set; }
    
    public BigInteger Importe { get; set; }
    
    public string NombreComercio { get; set; }
}