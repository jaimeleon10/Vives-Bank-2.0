using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Cliente.Controller;

[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly PaginationLinksUtils _paginationLinksUtils;


    public ClienteController(IClienteService clienteService, PaginationLinksUtils paginations)
    {
        _clienteService = clienteService;
        _paginationLinksUtils = paginations;
    }
    
    
    [HttpGet("page")]
    public async Task<ActionResult<List<PageResponse<ClienteResponse>>>> GetAllPaged(
        [FromQuery] string? nombre = null,
        [FromQuery] string? apellido = null,
        [FromQuery] string? dni = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "id",
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
            var pageResult = await _clienteService.GetAllPagedAsync(nombre, apellido, dni, pageRequest);
            
            var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
            Response.Headers.Add("link", linkHeader);
            
            return Ok(pageResult);

        }
        catch (ClienteNotFound e)
        {
            return StatusCode(404, new { message = "No se han encontrado los clientes.", details = e.Message });
        }
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
