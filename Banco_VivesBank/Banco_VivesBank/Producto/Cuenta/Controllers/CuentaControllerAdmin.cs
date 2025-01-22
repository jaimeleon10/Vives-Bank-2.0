using System.Numerics;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Services;
using Banco_VivesBank.Utils.Pagination;
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
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<CuentaResponse>>> Getall(
        [FromQuery] BigInteger? saldoMax = null,
        [FromQuery] BigInteger? saldoMin = null,
        [FromQuery] string? tipoCuenta = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "Id",
        [FromQuery] string direction = "asc")
    {
        try
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

            Response.Headers.Add("link", linkHeader);

            return Ok(pageResult);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }

    [HttpGet("allAcounts/{guid:length(12)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllByClientGuid(string guid)
    {
        try
        {
            var cuentas = await _cuentaService.GetByClientGuidAsync(guid);
            if (guid.Length!= 12) return BadRequest(new {message ="El guid debe tener 12 caracteres"});
            //if (guid is null) return NotFound($"No se ha encontrado el cliente con guid {guid}");
            //if (cuentas.Count() == 0) return NotFound($"No se han encontrado cuentas para el cliente con guid {guid}");
            return Ok(cuentas);
        }
        catch (ClienteNotFoundException e)
        {
            return NotFound( new { message = "Error buscando cliente.", details = e.Message });

        }
        catch (Exception e)
        {
            return BadRequest( new { message = "Error obteniendo las cuentas.", details = e.Message });
        }
        
    }

    [HttpGet("{guid:length(12)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByGuid(string guid)
    {
        var cuentaByGuid = await _cuentaService.GetByGuidAsync(guid);
        if (guid.Length != 12) return BadRequest(new { message = $"El guid debe tener 12 caracteres"});
        if (cuentaByGuid is null) return NotFound(new {message ="Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaByGuid);
    }
    
    [HttpGet("iban/{iban:length(34)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByIban(string iban)
    {
        var cuentaByIban = await _cuentaService.GetByIbanAsync(iban);
        if (iban.Length!= 34) return BadRequest(new {message = "El iban debe tener 34 caracteres"});
        if (cuentaByIban is null) return NotFound(new { message = $"Cuenta no encontrada con iban {iban}"});
        return Ok(cuentaByIban);
    }
    
    [HttpDelete("{guid:length(12)}")]
    public async Task<ActionResult<CuentaResponse>> Delete(string guid)
    {
        var cuentaDelete = await _cuentaService.DeleteAdminAsync(guid);
        if (guid.Length!= 12) return BadRequest(new {message ="El guid debe tener 12 caracteres"});
        if (cuentaDelete is null) return NotFound(new {message =$"Cuenta no encontrada con guid {guid}"});
        return Ok(cuentaDelete);
    }
}