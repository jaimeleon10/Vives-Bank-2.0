using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Mapper;

public static class MovimientoMapper
{
    public static MovimientoResponse ToResponseFromModel(
        this Movimiento movimiento, 
        DomiciliacionResponse? domiciliacionResponse, 
        IngresoNominaResponse? ingresoNominaResponse,
        PagoConTarjetaResponse? pagoConTarjetaResponse, 
        TransferenciaResponse? transferenciaResponse
    )
    {
        return new MovimientoResponse()
        {
            ClienteGuid = movimiento.Cliente.Guid,
            Domiciliacion = domiciliacionResponse,
            IngresoNomina = ingresoNominaResponse,
            PagoConTarjeta = pagoConTarjetaResponse,
            Transferencia = transferenciaResponse,
            CreatedAt = movimiento.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = movimiento.IsDeleted
        };
    }
}