using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Dto;

public class MovimientoResponse
{
    public required string ClienteGuid { get; set; }
    
    public DomiciliacionResponse? Domiciliacion { get; set; } = null;

    public IngresoNominaResponse? IngresoNomina { get; set; } = null;

    public PagoConTarjetaResponse? PagoConTarjeta { get; set; } = null;

    public TransferenciaResponse? Transferencia { get; set; } = null;
    
    public required string CreatedAt { get; set; }

    public required Boolean IsDeleted { get; set; }

}