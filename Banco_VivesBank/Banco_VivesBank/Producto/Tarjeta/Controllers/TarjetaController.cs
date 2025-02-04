using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Producto.Tarjeta.Services;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Banco_VivesBank.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Banco_VivesBank.Producto.Tarjeta.Controllers;

/// <summary>
/// Controlador para la gestión de tarjetas, incluyendo operaciones de obtención, creación, actualización y eliminación.
/// </summary>
[ApiController]
[Route ("api/tarjetas")]
public class TarjetaController : ControllerBase
{
    private readonly ITarjetaService _tarjetaService;
    private readonly CardLimitValidators _cardLimitValidators;
    private readonly ILogger<CardLimitValidators> _log;
    private readonly PaginationLinksUtils _paginationLinksUtils;
    private readonly IUserService _userService;

    public TarjetaController(ITarjetaService tarjetaService, ILogger<CardLimitValidators> log, PaginationLinksUtils pagination, IUserService userService)
    {
        _log = log; 
        _tarjetaService = tarjetaService;
        _cardLimitValidators = new CardLimitValidators(_log);
        _paginationLinksUtils = pagination;
        _userService = userService;
    }

    /// <summary>
    /// Obtiene una lista paginada de todas las tarjetas.
    /// </summary>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="size">Tamaño de la página.</param>
    /// <param name="sortBy">Campo por el cual ordenar los resultados.</param>
    /// <param name="direction">Dirección de ordenación (ascendente o descendente).</param>
    /// <returns>Una lista paginada de tarjetas.</returns>
    [SwaggerOperation(Summary = "Obtiene una lista paginada de todas las tarjetas.")]
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<IEnumerable<PageResponse<Models.Tarjeta>>>> GetAllTarjetas(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "id",
        [FromQuery] string direction = "asc")
    {
        
        var pageRequest = new PageRequest
        {
            PageNumber = page,
            PageSize = size,
            SortBy = sortBy,
            Direction = direction
        };
        var pageResult = await _tarjetaService.GetAllPagedAsync(pageRequest);
            
        var baseUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
        var linkHeader = _paginationLinksUtils.CreateLinkHeader(pageResult, baseUri);
            
        Response.Headers.Append("link", linkHeader);
            
        return Ok(pageResult);
    }

    
    /// <summary>
    /// Obtiene una tarjeta por su GUID.
    /// </summary>
    /// <param name="guid">El GUID de la tarjeta a obtener.</param>
    /// <returns>La tarjeta correspondiente, o un mensaje de error si no se encuentra.</returns>
    [HttpGet("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(Summary = "Obtiene una tarjeta por su GUID.")]
    public async Task<ActionResult<TarjetaResponse>> GetTarjetaByGuid(string guid)
    {
        var tarjeta = await _tarjetaService.GetByGuidAsync(guid);
        if (tarjeta == null) return NotFound($"La tarjeta con guid: {guid} no se ha encontrado");
        return Ok(tarjeta);
    }
    
    /// <summary>
    /// Obtiene una tarjeta por su número.
    /// </summary>
    /// <param name="numeroTarjeta">El número de la tarjeta a obtener.</param>
    /// <returns>La tarjeta correspondiente, o un mensaje de error si no se encuentra.</returns>
    [HttpGet("numero/{numeroTarjeta}")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(Summary = "Obtiene una tarjeta por su número.")]
    public async Task<ActionResult<TarjetaResponse>> GetTarjetaByNumeroTarjeta(string numeroTarjeta)
    {
        var tarjeta = await _tarjetaService.GetByNumeroTarjetaAsync(numeroTarjeta);
        if (tarjeta == null) return NotFound($"La tarjeta con numero de tarjeta: {numeroTarjeta} no se ha encontrado");
        return Ok(tarjeta);
    }

    /// <summary>
    /// Crea una nueva tarjeta.
    /// </summary>
    /// <param name="dto">Los datos necesarios para crear la tarjeta.</param>
    /// <returns>La tarjeta creada.</returns>
    [HttpPost]
    [Authorize(Policy = "ClientePolicy")]
    [SwaggerOperation(Summary = "Crea una nueva tarjeta.")]
    public async Task<ActionResult<Models.Tarjeta>> CreateTarjeta([FromBody] TarjetaRequest dto)
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
        if (userAuth.Role == Role.Admin) return BadRequest("El usuario es administrador. Un administrador no puede crear una tarjeta");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            if (dto.Pin.Length != 4)
            {
                return BadRequest("El pin tiene un formato incorrecto");
            }

            _cardLimitValidators.ValidarLimite(dto.LimiteDiario, dto.LimiteSemanal, dto.LimiteMensual);
            var tarjetaModel = await _tarjetaService.CreateAsync(dto, userAuth);
            
            return Ok(tarjetaModel);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }

    /// <summary>
    /// Actualiza una tarjeta existente.
    /// </summary>
    /// <param name="id">El ID de la tarjeta a actualizar.</param>
    /// <param name="dto">Los datos a actualizar.</param>
    /// <returns>La tarjeta actualizada.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ClientePolicy")]
    [SwaggerOperation(Summary = "Actualiza una tarjeta existente.")]
    public async Task<ActionResult<TarjetaResponse>> UpdateTarjeta(string id, [FromBody] TarjetaRequestUpdate dto)
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
        if (userAuth.Role == Role.Admin) return BadRequest("El usuario es administrador. Un administrador no puede modificar una tarjeta");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _cardLimitValidators.ValidarLimite(dto.LimiteDiario, dto.LimiteSemanal, dto.LimiteMensual);
            
            var tarjeta = await _tarjetaService.GetByGuidAsync(id);
            if (tarjeta == null) return NotFound($"La tarjeta con id: {id} no se ha encontrado");
            var updatedTarjeta = await _tarjetaService.UpdateAsync(id, dto, userAuth);
            return Ok(updatedTarjeta);
        }
        catch (Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }

    /// <summary>
    /// Elimina una tarjeta por su GUID.
    /// </summary>
    /// <param name="guid">El GUID de la tarjeta a eliminar.</param>
    /// <returns>El resultado de la eliminación.</returns>
    [HttpDelete("{guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(Summary = "Elimina una tarjeta por su GUID.")]
    public async Task<ActionResult<TarjetaResponse>> DeleteTarjeta(string guid)
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null) return NotFound("No se ha podido identificar al usuario logeado");
        if (userAuth.Role != Role.Admin || userAuth.Role != Role.Cliente) return BadRequest("Debes ser cliente o admin para borrar una tarjeta.");
        var tarjeta = await _tarjetaService.DeleteAsync(guid, userAuth);
        if (tarjeta == null) return NotFound($"La tarjeta con guid: {guid} no se ha encontrado");
        
        return Ok(tarjeta);
    }
}