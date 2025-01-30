/*using System.Security.Claims;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Cuenta.Controllers;

[ApiController]
[Route("api/cuentas/me")]
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
        try
        {
            return Ok(await _cuentaService.GetByClientGuidAsync(guid));
        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error obteniendo las cuentas.", details = e.Message });
        }
        
    }
    
    [HttpGet("/iban/{iban}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetMeByIban(
        //[FromServices] ClaimsPrincipal user,
        string iban
        )
    {
        try
        {
            var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
            if (cuentaByIban is null) return NotFound(new {menssage =$"Cuenta no encontrada con iban {iban}"});
            return Ok(cuentaByIban);
        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error obteniendo las cuentas.", details = e.Message });
        }/*
        catch (ClienteException e)
        {
            return NotFound( new { message = "Cuenta no encontrada del cliente.", details = e.Message });

        }
       
        
    }
    
    [HttpPost]
    //[Authorize]
    public async Task<ActionResult<CuentaResponse>> Create(
        //[FromServices] ClaimsPrincipal user,
        [FromBody] CuentaRequest cuentaRequest)
    {

        try
        {
            //var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            //var cuenta = await _cuentaService.CreateAsync(userId,cuentaRequest);
            var cuenta = await _cuentaService.CreateAsync(cuentaRequest);

            return Ok(cuenta);
        }
        catch (ProductoNotExistException e)
        {
            return NotFound( new { message = "Tipo de producto no existente.", details = e.Message });
        
        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error creando cuenta.", details = e.Message});

        }
        
    }
    
    [HttpDelete("{guid}")]
    //[Authorize]
    public async Task<ActionResult<CuentaResponse>> Delete(
        //[FromServices] ClaimsPrincipal user,
        string guid)
    {
        var cuentaDelete = await _cuentaService.DeleteByGuidAsync(guid);
        if (cuentaDelete is null) return NotFound($"Cuenta no encontrada con guid {guid}");
        return Ok(cuentaDelete);
    }
}
*/