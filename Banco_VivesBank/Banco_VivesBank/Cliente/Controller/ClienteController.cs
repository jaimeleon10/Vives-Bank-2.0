using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.User.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Cliente.Controller;

[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;


    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<ClienteResponse>>> GetAll()
    {
        return Ok(await _clienteService.GetAllAsync());
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<ClienteResponse>> GetByGuid(string guid)
    {
        var cliente = await _clienteService.GetByGuidAsync(guid);
     
        if (cliente is null) return NotFound($"No se ha encontrado cliente con guid: {guid}");
        
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create([FromBody] ClienteRequest clienteRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(await _clienteService.CreateAsync(clienteRequest));
        }
        catch (ClienteException e)
        {
            return BadRequest(e.Message);
        }
        catch (UserException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPut("{guid}")]
    public async Task<ActionResult<ClienteResponse>> UpdateCliente(string guid, [FromBody] ClienteRequestUpdate clienteRequestUpdate)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var clienteResponse = await _clienteService.UpdateAsync(guid, clienteRequestUpdate);
            if (clienteResponse is null) return NotFound($"No se ha podido actualizar el cliente con guid: {guid}"); 
            return Ok(clienteResponse);
        }
        catch (ClienteException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{guid}")]
    public async Task<ActionResult<ClienteResponse>> DeleteByGuid(string guid)
    {
        var clienteResponse = await _clienteService.DeleteByGuidAsync(guid);
        if (clienteResponse is null) return NotFound($"No se ha podido borrar el usuario con guid: {guid}"); 
        return Ok(clienteResponse);
    }

    [HttpPatch("{guid}/foto_perfil")]
    public async Task<ActionResult<ClienteResponse>> PatchFotoPerfil(string guid, IFormFile foto)
    {
        var clienteResponse = await _clienteService.UpdateFotoPerfil(guid, foto);
        
        if (clienteResponse is null) return NotFound($"No se ha podido actualizar la foto de perfil del cliente con guid: {guid}");
        return Ok(clienteResponse);
    }
    
    [HttpPatch("{guid}/foto_dni")]
    public async Task<ActionResult<ClienteResponse>> PatchFotoDni(string guid, IFormFile foto)
    {
        var clienteResponse = await _clienteService.UpdateFotoDni(guid, foto);
        
        if (clienteResponse is null) return NotFound($"No se ha podido actualizar la foto de perfil del cliente con guid: {guid}");
        return Ok(clienteResponse);
    }
}
