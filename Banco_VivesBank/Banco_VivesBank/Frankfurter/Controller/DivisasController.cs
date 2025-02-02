using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Model;
using Banco_VivesBank.Frankfurter.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Frankfurter.Controller;

/// <summary>
/// Controlador que maneja las solicitudes relacionadas con las tasas de cambio de divisas.
/// </summary>
[ApiController]
[Route("api/divisas")]
public class DivisasController : ControllerBase
{
    private readonly IDivisasService _divisasService;
    private readonly ILogger<DivisasController> _logger;

    /// <summary>
    /// Constructor para inicializar el controlador de divisas.
    /// </summary>
    /// <param name="divisasService">Servicio que maneja la lógica de obtención de tasas de cambio.</param>
    /// <param name="logger">Instancia del logger para registrar eventos y errores.</param>
    public DivisasController(IDivisasService divisasService, ILogger<DivisasController> logger)
    {
        _divisasService = divisasService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las últimas tasas de cambio desde una divisa base hacia una o varias divisas destino.
    /// </summary>
    /// <param name="amount">Cantidad de la divisa base a convertir. Por defecto es "1".</param>
    /// <param name="baseCurrency">Código de la divisa base. Por defecto es "EUR".</param>
    /// <param name="symbol">Código de la divisa destino (opcional). Si no se especifica, se obtienen todas las divisas posibles.</param>
    /// <returns>Una respuesta HTTP con las últimas tasas de cambio obtenidas.</returns>
    /// <remarks>
    /// Si ocurre un error al obtener las tasas de cambio, se lanza una excepción `FrankFurterConnectionException`.
    /// </remarks>
    [HttpGet("latest")]
    public ActionResult<FrankFurterResponse> GetLatestRates(
        [FromQuery] string amount = "1",
        [FromQuery] string baseCurrency = "EUR",
        [FromQuery] string? symbol = null)
    {
        _logger.LogInformation("Obteniendo las últimas tasas de cambio desde {BaseCurrency} a {Symbols}", baseCurrency, symbol);

        try
        {
            var result = _divisasService.ObtenerUltimasTasas(baseCurrency, symbol, amount);

            _logger.LogInformation("Respuesta construida: {@Result}", result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las últimas tasas de cambio.");
            throw new FrankFurterConnectionException(baseCurrency, symbol, ex);
        }
    }
}
