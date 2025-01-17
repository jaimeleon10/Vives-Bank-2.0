using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.Cliente.Dtos;
using Vives_Bank_Net.Rest.Cliente.Exceptions;
using Vives_Bank_Net.Rest.Cliente.Services;
using ILogger = DnsClient.Internal.ILogger;

namespace Vives_Bank_Net.Rest.Cliente.Controller;
[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClienteController> _logger;


    public ClienteController(IClienteService clienteService, ILogger<ClienteController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
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
     
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> SaveCliente([FromBody] ClienteRequestSave createDto)
    {

        if(!ModelState.IsValid)
        {
            throw new ClienteBadRequest("Los datos del cliente que intenta guardar son inválidos.");
        }
        var createdCliente = await _clienteService.CreateClienteAsync(createDto);
        return CreatedAtAction(nameof(GetClienteById), new { id = createdCliente.Id }, createdCliente);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteResponse>> UpdateCliente(string id, [FromBody]ClienteRequestUpdate updateDto)
    {
        if(!ModelState.IsValid)
        {
            throw new ClienteBadRequest("Los datos del cliente que intenta actualizar son inválidos.");
        }
        var clienteResponse = await _clienteService.UpdateClienteAsync(id, updateDto);
            return Ok(clienteResponse);
        
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ClienteResponse>> DeleteCliente(string id)
    {
        var deletedCliente = await _clienteService.DeleteClienteAsync(id);
        return Ok(deletedCliente);
    }
}
