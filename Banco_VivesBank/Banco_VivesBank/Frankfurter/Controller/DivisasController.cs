using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Model;
using Banco_VivesBank.Frankfurter.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Frankfurter.Controller;

[ApiController]
[Route("api/divisas")]
public class DivisasController : ControllerBase
{
    private readonly IDivisasService _divisasService;
    private readonly ILogger<DivisasController> _logger;

    public DivisasController(IDivisasService divisasService, ILogger<DivisasController> logger)
    {
        _divisasService = divisasService;
        _logger = logger;
    }

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
