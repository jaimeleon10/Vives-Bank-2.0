using System.Numerics;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
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


    public CuentaControllerAdmin(ICuentaService cuentaService, PaginationLinksUtils paginationLinksUtils)
    {
        _cuentaService = cuentaService;
        _paginationLinksUtils = paginationLinksUtils;
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
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

    [HttpGet("cliente/{guid}")]
    [Authorize(Roles = "Admin")]
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

    [HttpGet("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByGuid(string guid)
    {
        var cuentaByGuid = await _cuentaService.GetByGuidAsync(guid);
        if (cuentaByGuid is null) return NotFound(new {message =$"Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaByGuid);
    }
    
    [HttpGet("iban/{iban}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByIban(string iban)
    {
        var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
        if (cuentaByIban is null) return NotFound(new { message = $"Cuenta no encontrada con iban {iban}"});
        return Ok(cuentaByIban);
    }
    
    [HttpDelete("{guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CuentaResponse>> Delete(string guid)
    {
        var cuentaDelete = await _cuentaService.DeleteByGuidAsync(guid);
        if (cuentaDelete is null) return NotFound(new {message = $"Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaDelete);
    }
}