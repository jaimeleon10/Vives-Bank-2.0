using System.Security.Claims;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Cuenta.Controllers;

[ApiController]
[Route("api/me/cuentas")]
public class CuentaControllerMe: ControllerBase
{
    private readonly ICuentaService _cuentaService;

    public CuentaControllerMe(ICuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllMeAccounts(
        [FromServices] ClaimsPrincipal user,
        string guid
        )
    {
        return Ok(await _cuentaService.GetByClientGuidAsync(guid));
    }
    
    [HttpGet("/iban/{iban:length(34)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetMeByIban(
        //[FromServices] ClaimsPrincipal user,
        string iban
        )
    {
        var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
        if (iban.Length!= 34) return BadRequest($"El iban debe tener 34 caracteres");
        if (cuentaByIban is null) return NotFound($"Cuenta no encontrada con iban {iban}");
        return Ok(cuentaByIban);
    }
    
    [HttpPost]
    //[Authorize]
    public async Task<ActionResult<CuentaResponse>> Create(
        //[FromServices] ClaimsPrincipal user,
        [FromBody] CuentaRequest cuentaRequest)
    {
        //var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        //var cuenta = await _cuentaService.CreateAsync(userId,cuentaRequest);
        var cuenta = await _cuentaService.CreateAsync(cuentaRequest);

        return Ok(cuenta);
    }
    
    [HttpPut("{guid:length(12)}")]
    //[Authorize]
    public async Task<ActionResult<CuentaResponse>> Update(
        //[FromServices] ClaimsPrincipal user,
        [FromBody] CuentaUpdateRequest cuentaRequest,
        string guid)
    {
        //var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
             //var cuenta = await _cuentaService.UpdateAsync(userId,guid,cuentaRequest);
             var cuenta = await _cuentaService.UpdateAsync(guid,cuentaRequest);

             return Ok(cuenta);
        }
        catch (CuentaException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpDelete("{guid:length(12)}")]
    //[Authorize]
    public async Task<ActionResult<CuentaResponse>> Delete(
        //[FromServices] ClaimsPrincipal user,
        string guid)
    {
        var cuentaDelete = await _cuentaService.DeleteAdminAsync(guid);
        if (guid.Length!= 12) return BadRequest($"El guid debe tener 12 caracteres");
        if (cuentaDelete is null) return NotFound($"Cuenta no encontrada con guid {guid}");
        return Ok(cuentaDelete);
    }
}