using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Cliente.Services;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Pdf.Services;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Cliente.Controller;

[ApiController]
[Route("api/clientes")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IPdfStorage _pdfStorage;
    private readonly IMovimientoService _movimientoService;
    private readonly PaginationLinksUtils _paginationLinksUtils;
    private readonly IUserService _userService;

    public ClienteController(IClienteService clienteService, PaginationLinksUtils paginations
        , IPdfStorage pdfStorage, IMovimientoService movimientoService, IUserService userService)
    {
        _clienteService = clienteService;
        _paginationLinksUtils = paginations;
        _pdfStorage = pdfStorage;
        _movimientoService = movimientoService;
        _userService = userService;
    }
    
    
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
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
            
            Response.Headers.Append("link", linkHeader);
            
            return Ok(pageResult);

        }
        catch (ClienteNotFound e)
        {
            return StatusCode(404, new { message = "No se han encontrado los clientes.", details = e.Message });
        }
    }
    
    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ClienteResponse>> GetByGuid(string guid)
    {
        var cliente = await _clienteService.GetByGuidAsync(guid);
     
        if (cliente is null) return NotFound($"No se ha encontrado cliente con guid: {guid}");
        
        return Ok(cliente);
    }
    
    [HttpGet("me")]
    [Authorize(Policy = "ClientePolicy")]
    public async Task<ActionResult<ClienteResponse>> GetMyCliente()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
        
        var cliente = await _clienteService.GetMyClienteAsync(userAuth);
     
        if (cliente is null) return NotFound($"No se ha encontrado el cliente correspondiente al usuario con guid: {userAuth.Guid}");
        
        return Ok(cliente);
    }

    [HttpPost]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<ClienteResponse>> Create([FromBody] ClienteRequest clienteRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userAuth = _userService.GetAuthenticatedUser();
            if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
            
            return Ok(await _clienteService.CreateAsync(userAuth, clienteRequest));
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
    [Authorize(Policy = "AdminPolicy")]
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
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ClienteResponse>> DeleteByGuid(string guid)
    {
        var clienteResponse = await _clienteService.DeleteByGuidAsync(guid);
        if (clienteResponse is null) return NotFound($"No se ha podido borrar el usuario con guid: {guid}"); 
        return Ok(clienteResponse);
    }

    [HttpPatch("{guid}/foto_perfil")]
    public async Task<ActionResult<ClienteResponse>> PatchFotoPerfil(string guid, IFormFile foto)
    {
        try
        {
            var clienteResponse = await _clienteService.UpdateFotoPerfil(guid, foto);

            if (clienteResponse is null)
                return NotFound($"No se ha podido actualizar la foto de perfil del cliente con GUID: {guid}");

            return Ok(clienteResponse);
        }
        catch (FileStorageException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("{guid}/foto_dni")]
    public async Task<ActionResult<ClienteResponse>> PostFotoDni(string guid, IFormFile foto)
    {
        if (foto == null || foto.Length == 0)
        {
            return BadRequest("Debe proporcionar una foto válida para actualizar el DNI.");
        }
        
        try
        {
            var clienteResponse = await _clienteService.UpdateFotoDni(guid, foto);
            return Ok(clienteResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"Ocurrió un error al actualizar la foto del DNI: {ex.Message}");
        }
    }

    [HttpGet("{guid}/foto_dni")]
    public async Task<IActionResult> GetFotoDni(string guid)
    {
        try
        {
            var fotoStream = await _clienteService.GetFotoDniAsync(guid);
        
            if (fotoStream == null)
                return NotFound("No se encontró la foto del DNI para este cliente.");

            return File(fotoStream, "image/jpeg");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error interno al recuperar la foto del DNI.");
        }
    }
    
    [HttpGet("{guid}/movimientos_pdf")]
    public async Task<ActionResult<List<MovimientoResponse>>> GetMovimientos(string guid)
    {
        var clienteResponse = await _clienteService.GetByGuidAsync(guid);
        if (clienteResponse is null)
            return NotFound($"No se ha encontrado cliente con guid: {guid}");

        var movimientos = await _movimientoService.GetByClienteGuidAsync(guid);

        if (movimientos is null || !movimientos.Any())
            return NotFound($"No se han encontrado movimientos del cliente con guid: {guid}");

        var movimientosList = movimientos.ToList();

        _pdfStorage.ExportPDF(clienteResponse, movimientosList);
        return Ok(movimientosList);
    }

}
