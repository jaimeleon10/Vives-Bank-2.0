using Banco_VivesBank.Movimientos.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Movimientos;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de IngresoNominaResponse
/// </summary>
public sealed class IngresoNominaResponseExample : IExamplesProvider<IngresoNominaResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de IngresoNominaResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de IngresoNominaResponse</returns>
    public IngresoNominaResponse GetExamples()
    {
        return new IngresoNominaResponse
        {
            NombreEmpresa = "Empresa Nómina Ejemplo",
            CifEmpresa = "A12345678",
            IbanEmpresa = "ES7604878673285989969615",
            IbanCliente = "ES7730046576085345979538",
            Importe = 3000.00,
        };
    }
}