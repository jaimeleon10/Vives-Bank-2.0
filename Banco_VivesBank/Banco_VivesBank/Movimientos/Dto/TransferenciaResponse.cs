using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class TransferenciaResponse
{
    public string IbanOrigen { get; set; }
    
    public string IbanDestino { get; set; }

    public BigInteger Importe { get; set; }
    
    public string NombreBeneficiario { get; set; }
}