using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.Producto.Cuenta.Dto;
using Vives_Bank_Net.Rest.Producto.Cuenta.Services;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Controllers;

[ApiController]
[Route("api/me/accounts")]
public class CuentaControllerMe: ControllerBase
{
    private readonly ICuentaService _cuentaService;
    private readonly ILogger<CuentaControllerAdmin> _logger;


    public CuentaControllerMe(ICuentaService cuentaService, ILogger<CuentaControllerAdmin> logger)
    {
        _cuentaService = cuentaService;
        _logger = logger;
       
    }
    
    
    [HttpGet]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllMeAccounts(
        [FromServices] ClaimsPrincipal user,
        string guid
        )
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation($"Buscando todos mis Cuentas {userId}");

        try
        {
            var cuentas = await _cuentaService.getByClientGuid(guid);

            return Ok(cuentas);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener las cuentas del cliente {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }

    }
    
    [HttpGet("/iban/{iban:length(34)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetMeByIban(
        [FromServices] ClaimsPrincipal user,
        string iban
        )
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban}");
        try
        {
            var cuenta = await _cuentaService.getMeByIban(userId,iban);
            return Ok(cuenta);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la cuenta {iban}.", iban);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });

        }

    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CuentaResponse>> Create(
        [FromServices] ClaimsPrincipal user,
        [FromBody] CuentaRequest cuentaRequest)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Creando cuenta");
        
        var cuenta = await _cuentaService.save(userId,cuentaRequest);
        return Ok(cuenta);
    }
    
    [HttpPut("{guid:length(12)}")]
    [Authorize]
    public async Task<ActionResult<CuentaResponse>> Update(
        [FromServices] ClaimsPrincipal user,
        [FromBody] CuentaUpdateRequest cuentaRequest,
        string guid)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Actualizando cuenta {guid}",guid);
        try
        {

             var cuenta = await _cuentaService.update(userId,guid,cuentaRequest);
             return Ok(cuenta);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar la cuenta {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
       
    }
    
    
    [HttpDelete("{guid:length(12)}")]
    [Authorize]
    public async Task<ActionResult<CuentaResponse>> Delete(
        [FromServices] ClaimsPrincipal user,
        string guid)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Eliminando cuenta {guid}",guid);
        try
        {

            var cuenta = await _cuentaService.delete(userId,guid);
            return Ok(cuenta);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al borrar la cuenta {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
       
    }
    
    
    
}