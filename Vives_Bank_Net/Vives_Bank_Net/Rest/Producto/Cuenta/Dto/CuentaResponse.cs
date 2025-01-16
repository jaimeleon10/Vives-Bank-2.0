using System.Numerics;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Dto;

public class CuentaResponse
{
    public required string Guid { get; set; }
    
    public required string Iban { get; set; } 
    
    public BigInteger Saldo { get; set; }

    public long? TarjetaId { get; set; }
    
    public long ClienteId { get; set; } 
    
    public long ProductoId { get; set; }
    
}