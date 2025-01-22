using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Mapper;

public static class PagoConTarjetaMapper
{
    public static PagoConTarjetaResponse? ToResponseFromModel(this PagoConTarjeta? pagoConTarjeta)
    {
        if (pagoConTarjeta == null)
        {
            return null;
        }
        return new PagoConTarjetaResponse()
        {
            NumeroTarjeta = pagoConTarjeta.NumeroTarjeta,
            Importe = pagoConTarjeta.Importe,
            NombreComercio = pagoConTarjeta.NombreComercio
        };
    }
}