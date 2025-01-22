using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Mapper;

public static class IngresoNominaMapper
{
    public static IngresoNominaResponse? ToResponseFromModel(this IngresoNomina? ingresoNomina)
    {
        if (ingresoNomina == null)
        {
            return null;
        }
        return new IngresoNominaResponse()
        {
            IbanOrigen = ingresoNomina.IbanOrigen,
            IbanDestino = ingresoNomina.IbanDestino,
            Importe = ingresoNomina.Importe,
            NombreEmpresa = ingresoNomina.NombreEmpresa,
            CifEmpresa = ingresoNomina.CifEmpresa
        };
    }
}