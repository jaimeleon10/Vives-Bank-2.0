﻿using Banco_VivesBank.Movimientos.Dto;
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
            ClienteGuid = domiciliacion.ClienteGuid,
            Acreedor = domiciliacion.Acreedor,
            IbanEmpresa = domiciliacion.IbanEmpresa,
            IbanCliente = domiciliacion.IbanCliente,
            Importe = domiciliacion.Importe.ToString(),
            Periodicidad = domiciliacion.Periodicidad.ToString(),
            Activa = domiciliacion.Activa,
            FechaInicio = domiciliacion.FechaInicio.ToString("dd/MM/yyyy - HH:mm:ss"),
            UltimaEjecuccion = domiciliacion.UltimaEjecucion.ToString("dd/MM/yyyy - HH:mm:ss")
        };
    }
}