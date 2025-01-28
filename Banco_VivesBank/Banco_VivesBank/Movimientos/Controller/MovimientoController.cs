using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Exceptions;
using Banco_VivesBank.Movimientos.Services;
using Banco_VivesBank.Movimientos.Services.Movimientos;
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
        return Ok(await _movimientoService.GetByClienteGuidAsync(clienteGuid));
    }
    
    [HttpPost("ingresoNomina")]
    public async Task<ActionResult<IngresoNominaResponse>> CreateIngresoNomina([FromBody] IngresoNominaRequest ingresoNominaRequest)
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
    public async Task<ActionResult<PagoConTarjetaResponse>> CreatePagoConTarjeta([FromBody] PagoConTarjetaRequest pagoConTarjetaRequest)
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
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(e.Message);
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
    public async Task<ActionResult<TransferenciaResponse>> CreateTransferencia([FromBody] TransferenciaRequest transferenciaRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _movimientoService.CreateTransferenciaAsync(transferenciaRequest));
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(e.Message);
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
    
    [HttpPost("transferencia/revocar/{movimientoGuid}")]
    public async Task<ActionResult<TransferenciaResponse>> RevocarTransferencia(string movimientoGuid)
    {
        try
        {
            return Ok(await _movimientoService.RevocarTransferenciaAsync(movimientoGuid));
        }
        catch (MovimientoNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (MovimientoException e)
        {
            return BadRequest(e.Message);
        }
        catch (SaldoCuentaInsuficientException e)
        {
            return BadRequest(e.Message);
        }
        catch (CuentaException e)
        {
            return NotFound(e.Message);
        }
    }
}