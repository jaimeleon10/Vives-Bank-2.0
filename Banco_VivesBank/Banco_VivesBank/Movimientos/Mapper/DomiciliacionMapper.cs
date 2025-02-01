using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Mapper;

public static class DomiciliacionMapper
{
    public static DomiciliacionResponse? ToResponseFromModel(this Domiciliacion? domiciliacion)
    {
        if (domiciliacion == null)
        {
            return null;
        }
        
        return new DomiciliacionResponse()
        {
            Guid = domiciliacion.Guid,
            ClienteGuid = domiciliacion.ClienteGuid,
            Acreedor = domiciliacion.Acreedor,
            IbanEmpresa = domiciliacion.IbanEmpresa,
            IbanCliente = domiciliacion.IbanCliente,
            Importe = domiciliacion.Importe,
            Periodicidad = domiciliacion.Periodicidad.ToString(),
            Activa = domiciliacion.Activa,
            FechaInicio = domiciliacion.FechaInicio.ToString(),
            UltimaEjecuccion = domiciliacion.UltimaEjecucion.ToString()
        };
    }
}