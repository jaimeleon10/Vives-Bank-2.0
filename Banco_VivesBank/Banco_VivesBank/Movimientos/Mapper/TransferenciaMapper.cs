using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Mapper;

public static class TransferenciaMapper
{
    public static TransferenciaResponse? ToResponseFromModel(this Transferencia? transferencia)
    {
        if (transferencia == null)
        {
            return null;
        }
        return new TransferenciaResponse()
        {
            ClienteOrigen = transferencia.ClienteOrigen,
            IbanOrigen = transferencia.IbanOrigen,
            NombreBeneficiario = transferencia.NombreBeneficiario,
            IbanDestino = transferencia.IbanDestino,
            Importe = transferencia.Importe,
            Revocada = transferencia.Revocada
        };
    }
}