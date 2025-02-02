using Banco_VivesBank.Movimientos.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Movimientos;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de TransferenciaResponse
/// </summary>
public sealed class TransferenciaResponseExample : IExamplesProvider<TransferenciaResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de TrasnferenciaResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de TrasnferenciaResponse</returns>
    public TransferenciaResponse GetExamples()
    {
        return new TransferenciaResponse
        {
            ClienteOrigen = "Pedro Picapiedra",
            IbanOrigen = "ES7730046576085345979538",
            NombreBeneficiario = "Ana Martinez",
            IbanDestino = "ES2114656261103572788444",
            Importe = 1000.00,
            Revocada = false,
        };
    }
}