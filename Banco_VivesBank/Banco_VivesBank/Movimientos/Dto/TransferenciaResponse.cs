using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class TransferenciaResponse
{
    public string ClienteOrigen { get; set; }
    
    public string IbanOrigen { get; set; }
    
    public string NombreBeneficiario { get; set; }
    
    public string IbanDestino { get; set; }

    public double Importe { get; set; }
 
    public bool Revocada { get; set; }
}