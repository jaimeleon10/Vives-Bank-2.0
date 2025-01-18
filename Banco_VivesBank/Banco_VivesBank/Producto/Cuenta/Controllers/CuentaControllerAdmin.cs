using System.Numerics;
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
    private readonly ILogger<CuentaControllerAdmin> _logger;


    public CuentaControllerAdmin(ICuentaService cuentaService, PaginationLinksUtils paginationLinksUtils,
        ILogger<CuentaControllerAdmin> logger)
    {
        _cuentaService = cuentaService;
        _paginationLinksUtils = paginationLinksUtils;
        _logger = logger;
    }


    [HttpGet]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<CuentaResponse>>> Getall(
        [FromQuery] BigInteger? saldoMax = null,
        [FromQuery] BigInteger? saldoMin = null,
        [FromQuery] string? tipoCuenta = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "id",
        [FromQuery] string direction = "asc")
    {
        try
        {
            _logger.LogInformation(
                "Buscando todas las cuentas con las siguientes opciones:SaldoMax: {saldoMax}, SaldoMin: {saldoMin}, TipoCuenta: {tipoCuenta}",
                saldoMax, saldoMin, tipoCuenta);

            var pageRequest = new PageRequest
            {
                PageNumber = page,
                PageSize = size,
                SortBy = sortBy,
                Direction = direction
            };

            var pageResult = await _cuentaService.GetAll(saldoMax, saldoMin, tipoCuenta, pageRequest);

            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);

            Response.Headers.Add("link", linkHeader);

            return Ok(pageResult);
        }
        catch (CuentaNoEncontradaException e)
        {
            _logger.LogError(e, "No se ha encontrado ninguna cuenta.");
            return StatusCode(404, new { message = "No se han encontrado las cuentas.", details = e.Message });

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener las cuentas.");
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }

    [HttpGet("allAcounts/{guid:length(12)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetAllByClientGuid(string guid)
    {
        _logger.LogInformation($"Buscando todos las Cuentas del cliente {guid}");

        try
        {
            var cuentas = await _cuentaService.getByClientGuid(guid);

            return Ok(cuentas);
        }
        catch (CuentaInvalidaException e)
        {
            _logger.LogError(e, "Cuentas no encontradas por guid del cliente invalido.");
            return StatusCode(400, new { message = "Cuentas no encontradas por guid del cliente invalido.", details = e.Message });

        }
        catch (CuentaNoEncontradaException e)
        {
            _logger.LogError(e, "No se ha encontrado ninguna cuenta del cliente con guid {guid}.", guid);
            return StatusCode(404, new { message = "No se han encontrado las cuentas del cliente.", details = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener las cuentas del cliente {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }

    [HttpGet("{guid:length(12)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cuenta: {guid}");
        try
        {
            var cuentas = await _cuentaService.getByGuid(guid);
            return Ok(cuentas);
        }
        catch (CuentaInvalidaException e)
        {
            _logger.LogError(e, "Cuenta no encontrada por guid invalido.");
            return StatusCode(400, new { message = "Cuenta no encontrada por guid invalido.", details = e.Message });

        }
        catch (CuentaNoEncontradaException e)
        {
            _logger.LogError(e, "No se ha encontrado la cuenta con guid {guid}.", guid);
            return StatusCode(404, new { message = "No se ha encontrado la cuenta.", details = e.Message });

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la cuenta {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });

        }
    }
    
    [HttpGet("iban/{iban:length(34)}")]
    public async Task<ActionResult<List<CuentaResponse>>> GetByIban(string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban}");
        try
        {
            var cuentas = await _cuentaService.getByIban(iban);
            return Ok(cuentas);
        }
        catch (CuentaInvalidaException e)
        {
            _logger.LogError(e, "Cuenta no encontrada por iban invalido.");
            return StatusCode(400, new { message = "Cuenta no encontrada por iban invalido.", details = e.Message });

        }
        catch (CuentaNoEncontradaException e)
        {
            _logger.LogError(e, "No se ha encontrado la cuenta con iban {iban}.", iban);
            return StatusCode(404, new { message = "No se ha encontrado la cuenta.", details = e.Message });

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la cuenta {iban}.", iban);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });

        }
    }
    
    [HttpDelete("{guid:length(12)}")]
    public async Task<ActionResult<CuentaResponse>> Delete(string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}",guid);
        try
        {

            var cuenta = await _cuentaService.deleteAdmin(guid);
            return Ok(cuenta);
        }
        catch (CuentaNoEncontradaException e)
        {
            _logger.LogError(e, "No se ha encontrado la cuenta con guid {guid}.", guid);
            return StatusCode(404, new { message = "No se ha encontrado la cuenta.", details = e.Message });

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al borrar la cuenta {guid}.", guid);
            return StatusCode(500, new { message = "Ocurrió un error procesando la solicitud.", details = e.Message });
        }
    }
}