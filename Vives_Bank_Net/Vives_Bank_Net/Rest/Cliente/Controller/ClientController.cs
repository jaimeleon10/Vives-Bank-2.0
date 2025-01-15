using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.Cliente.Dtos;
using Vives_Bank_Net.Rest.Cliente.Exceptions;
using Vives_Bank_Net.Rest.Cliente.Services;

namespace Vives_Bank_Net.Rest.Cliente.Controller;
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }


    [HttpGet]
    public async Task<ActionResult<List<ClienteResponse>>> GetAllClientes()
    {
        return Ok(await _clienteService.GetAllClientesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteResponse>> GetClienteById(string id)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id);
        if (cliente == null) return NotFound("El cliente con el id "+ id + " no existe");
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> SaveCliente([FromBody] ClienteRequestSave createDto)
    {
        return null;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteResponse>> UpdateCliente(string id, [FromBody] ClienteRequestSave updateDto)
    {
        return null;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ClienteResponse>> DeleteCliente(string id)
    {
        return null;
    }
}