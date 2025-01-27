using System.Numerics;

namespace Banco_VivesBank.Movimientos.Dto;

public class DomiciliacionResponse
{
    public required string Guid { get; set; }
    
    public string ClienteGuid { get; set; }
    
    public string Acreedor { get; set; }
    
    public string IbanEmpresa { get; set; }
    
    public string IbanCliente { get; set; }
    
    public double Importe { get; set; }
    
    public string Periodicidad { get; set; }

    public bool Activa { get; set; } = true;
    
    public string FechaInicio { get; set; }
    
    public string UltimaEjecuccion { get; set; }
}