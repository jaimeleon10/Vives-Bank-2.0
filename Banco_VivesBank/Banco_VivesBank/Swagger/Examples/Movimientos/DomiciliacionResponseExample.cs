using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Movimientos;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de DomiciliacionResponse
/// </summary>
public sealed class DomiciliacionResponseExample : IExamplesProvider<DomiciliacionResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de DomiciliacionResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de DomiciliacionResponse</returns>
    public DomiciliacionResponse GetExamples()
    {
        return new DomiciliacionResponse
        {
            Guid = "1t2gVegRt2x",
            ClienteGuid = "GrFprHzywot",
            Acreedor = "Netflix",
            IbanEmpresa = "ES9520954643908286752268",
            IbanCliente = "ES9520954643908286752268",
            Importe = 12.50,
            Periodicidad = Periodicidad.Semanal.ToString(),
            Activa = true,
            FechaInicio = DateTime.UtcNow.ToString(),
            UltimaEjecuccion = DateTime.UtcNow.ToString()
        };
    }
}