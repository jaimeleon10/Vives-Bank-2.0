using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class IngresoNominaResponse
{
    public string IbanOrigen { get; set; }
    
    public string IbanDestino { get; set; }
    
    public BigInteger Importe { get; set; }
    
    public string NombreEmpresa { get; set; }
    
    public string CifEmpresa { get; set; }
}