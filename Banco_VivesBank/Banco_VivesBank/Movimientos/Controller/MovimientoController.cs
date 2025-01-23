using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Movimientos.Controller;

[ApiController]
[Route("api/movimientos")]
public class MovimientoController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;
    
    public MovimientoController(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetAll()
    {
        return Ok(await _movimientoService.GetAllAsync());
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<MovimientoResponse>> GetById(string guid)
    {
        var movimiento = await _movimientoService.GetByGuidAsync(guid);
        
        if (movimiento == null) return NotFound($"No se ha encontrado el movimiento con guid: {guid}");
        
        return Ok(movimiento);
    }
    
    [HttpGet("cliente/{clienteGuid}")]
    public async Task<ActionResult<IEnumerable<MovimientoResponse>>> GetByClienteGuid(string clienteGuid)
    {
        return Ok(await _movimientoService.GetByGuidAsync(clienteGuid));
    }

    [HttpPost("domiciliacion")]
    public async Task<ActionResult<MovimientoResponse>> CreateDomiciliacion([FromBody] DomiciliacionRequest domiciliacionRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _movimientoService.CreateDomiciliacionAsync(domiciliacionRequest));
        }
        catch (ClienteException e)
        {
            return NotFound(e.Message);
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("ingresoNomina")]
    public async Task<ActionResult<MovimientoResponse>> CreateIngresoNomina([FromBody] IngresoNominaRequest ingresoNominaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _movimientoService.CreateIngresoNominaAsync(ingresoNominaRequest));
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("pagoConTarjeta")]
    public async Task<ActionResult<MovimientoResponse>> CreatePagoConTarjeta([FromBody] PagoConTarjetaRequest pagoConTarjetaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _movimientoService.CreatePagoConTarjetaAsync(pagoConTarjetaRequest));
        }
        catch (TarjetaException e)
        {
            return NotFound(e.Message);
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("transferencia")]
    public async Task<ActionResult<MovimientoResponse>> CreateTransferencia([FromBody] TransferenciaRequest transferenciaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _movimientoService.CreateTransferenciaAsync(transferenciaRequest));
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("transferencia/revocar")]
    public async Task<ActionResult<MovimientoResponse>> RevocarTransferencia(string movimientoGuid)
    {
        try
        {
            return Ok(await _movimientoService.RevocarTransferenciaAsync(movimientoGuid));
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
    }
}