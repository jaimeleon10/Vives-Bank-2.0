using Banco_VivesBank.Movimientos.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Movimientos;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de MovimientoResponse
/// </summary>
public sealed class MovimientoResponseExample : IExamplesProvider<MovimientoResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de MovimientoResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de MovimientoResponse</returns>
    public MovimientoResponse GetExamples()
    {
        return new MovimientoResponse
        {
            Guid = "iFDVeS3riQn",
            ClienteGuid = "GbJtJkggUOM",
            Domiciliacion = null,
            IngresoNomina = null,
            PagoConTarjeta = null,
            Transferencia = new TransferenciaResponse
            {
                ClienteOrigen = "Pedro Picapiedra",
                IbanOrigen = "ES7730046576085345979538",
                NombreBeneficiario = "Ana Martinez",
                IbanDestino = "ES2114656261103572788444",
                Importe = 1000.00,
                Revocada = false
            },
            CreatedAt = DateTime.UtcNow.ToString(),
        };
    }
}