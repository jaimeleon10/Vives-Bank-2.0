using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class IngresoNominaResponse
{
    public string NombreEmpresa { get; set; }

    public string CifEmpresa { get; set; }

    public string IbanEmpresa { get; set; }
    
    public string IbanCliente { get; set; }
    
    public double Importe { get; set; }
}