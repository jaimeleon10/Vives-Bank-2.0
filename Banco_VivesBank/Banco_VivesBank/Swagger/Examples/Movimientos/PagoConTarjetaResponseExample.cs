using Banco_VivesBank.Movimientos.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Movimientos;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de PagoConTarjetaResponse
/// </summary>
public sealed class PagoConTarjetaResponseExample : IExamplesProvider<PagoConTarjetaResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de PagoConTarjetaResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de PagoConTarjetaResponse</returns>
    public PagoConTarjetaResponse GetExamples()
    {
        return new PagoConTarjetaResponse
        {
            NombreComercio = "Supermercado Ejemplo",
            Importe = 200.00,
            NumeroTarjeta = "0606579225434779"
        };
    }
}