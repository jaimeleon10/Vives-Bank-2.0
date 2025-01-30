using System.Numerics;
using System.Security.Claims;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Producto.ProductoBase.Exceptions;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Producto.Cuenta.Controllers;

[ApiController]
[Route("api/cuentas")]
public class CuentaControllerAdmin : ControllerBase
{

    private readonly ICuentaService _cuentaService;
    private readonly PaginationLinksUtils _paginationLinksUtils;
    private readonly IUserService _userService;



    public CuentaControllerAdmin(ICuentaService cuentaService, PaginationLinksUtils paginationLinksUtils,IUserService userService)
    {
        _cuentaService = cuentaService;
        _paginationLinksUtils = paginationLinksUtils;
        _userService = userService;
    }


    [HttpGet("admin")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<CuentaResponse>>> Getall(
        [FromQuery] double? saldoMax = null,
        [FromQuery] double? saldoMin = null,
        [FromQuery] string? tipoCuenta = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "Id",
        [FromQuery] string direction = "asc")
    {
      
            var pageRequest = new PageRequest
            {
                PageNumber = page,
                PageSize = size,
                SortBy = sortBy,
                Direction = direction
            };

            var pageResult = await _cuentaService.GetAllAsync(saldoMax, saldoMin, tipoCuenta, pageRequest);

            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);

            Response.Headers.Append("link", linkHeader);

            return Ok(pageResult);
        
    }

    [HttpGet("admin/cliente/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllByClientGuid(string guid)
    {
        try
        {
            var cuentas = await _cuentaService.GetByClientGuidAsync(guid);
            return Ok(cuentas);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound( new { message = "Error buscando cliente.", details = e.Message });
        }
    }

    [HttpGet("admin/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByGuid(string guid)
    {
        var cuentaByGuid = await _cuentaService.GetByGuidAsync(guid);
        if (cuentaByGuid is null) return NotFound(new {message =$"Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaByGuid);
    }
    
    [HttpGet("admin/iban/{iban}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByIban(string iban)
    {
        var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
        if (cuentaByIban is null) return NotFound(new { message = $"Cuenta no encontrada con iban {iban}"});
        return Ok(cuentaByIban);
    }
    
    [HttpDelete("admin/{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<CuentaResponse>> DeleteAdmin(string guid)
    {
        var cuentaDelete = await _cuentaService.DeleteByGuidAsync(guid);
        if (cuentaDelete is null) return NotFound(new {message = $"Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaDelete);
    }
    
     [HttpGet("me")]
     [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllMeAccounts()
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new {message = "No se ha podido identificar al usuario logeado"});
            
            return Ok(await _cuentaService.GetByClientGuidAsync(userAuth.Guid));
        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error obteniendo las cuentas.", details = e.Message });
        }
        
    }
    

    [HttpGet("/iban/{iban}")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<List<CuentaResponse>>> GetMeByIban(string iban)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new {message = "No se ha podido identificar al usuario logeado"});
            
            
            var cuentaByIban = await _cuentaService.GetMeByIbanAsync(userAuth.Guid,iban);
            if (cuentaByIban is null) return NotFound(new {menssage =$"Cuenta no encontrada con iban {iban}"});
            return Ok(cuentaByIban);
        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error obteniendo las cuentas.", details = e.Message });
        }
        
    }
    
    [HttpPost("me")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<CuentaResponse>> Create([FromBody] CuentaRequest cuentaRequest)
    {
        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound(new {message = "No se ha podido identificar al usuario logeado"});
            
            var cuenta = await _cuentaService.CreateAsync(userAuth.Guid,cuentaRequest);

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
    
    [HttpDelete("me/{guid}")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<CuentaResponse>> Delete(string guid)
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound(new {message = "No se ha podido identificar al usuario logeado"});
        
        var cuentaDelete = await _cuentaService.DeleteMeAsync(userAuth.Guid,guid);
        if (cuentaDelete is null) return NotFound($"Cuenta no encontrada con guid {guid}");
        return Ok(cuentaDelete);
    }
}