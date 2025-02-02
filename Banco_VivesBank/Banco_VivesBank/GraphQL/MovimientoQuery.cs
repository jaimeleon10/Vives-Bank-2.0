using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Services.Domiciliaciones;
using Banco_VivesBank.Movimientos.Services.Movimientos;
using Banco_VivesBank.User.Service;
using Microsoft.AspNetCore.Authorization;

namespace Banco_VivesBank.GraphQL;

public class MovimientoQuery
{
    private readonly IMovimientoService _movimientoService;
    private readonly IDomiciliacionService _domiciliacionService;
    private readonly IUserService _userService;


    public MovimientoQuery(IMovimientoService movimientoService, IDomiciliacionService domiciliacionService, IUserService userService)
    {
        _movimientoService = movimientoService;
        _domiciliacionService = domiciliacionService;
        _userService = userService;
    }

    [GraphQLName("movimientos")]
    public async Task<IEnumerable<MovimientoResponse>> GetMovimientosAsync() =>
        await _movimientoService.GetAllAsync();
    
    [GraphQLName("movimientoByGuid")]
    public async Task<MovimientoResponse?> GetMovimientoByGuidAsync(string guid) =>
        await _movimientoService.GetByGuidAsync(guid);

    [GraphQLName("movimientosByClienteGuid")]
    public async Task<IEnumerable<MovimientoResponse>> GetMovimientosByClienteGuidAsync(string clienteGuid) =>
        await _movimientoService.GetByClienteGuidAsync(clienteGuid);
    
    [Authorize(Policy = "ClientePolicy")]
    [GraphQLName("misMovimientos")]
    public async Task<IEnumerable<MovimientoResponse>> GetMyMovimientosAsync()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null)
        {
            throw new GraphQLException("No se ha podido identificar al usuario autenticado.");
        }

        return await _movimientoService.GetMyMovimientos(userAuth);
    }
    
    [GraphQLName("domiciliaciones")]
    public async Task<IEnumerable<DomiciliacionResponse>> GetDomiciliacionesAsync() =>
        await _domiciliacionService.GetAllAsync();

    [GraphQLName("domiciliacionByGuid")]
    public async Task<DomiciliacionResponse?> GetDomiciliacionByGuidAsync(string domiciliacionGuid) =>
        await _domiciliacionService.GetByGuidAsync(domiciliacionGuid);

    [GraphQLName("domiciliacionesByClienteGuid")]
    public async Task<IEnumerable<DomiciliacionResponse>> GetDomiciliacionesByClienteGuidAsync(string clienteGuid) =>
        await _domiciliacionService.GetByClienteGuidAsync(clienteGuid);
    
    [Authorize(Policy = "ClientePolicy")]
    [GraphQLName("misDomiciliaciones")]
    public async Task<IEnumerable<DomiciliacionResponse>> GetMyDomiciliacionesAsync()
    {
        var userAuth = _userService.GetAuthenticatedUser();
        if (userAuth is null)
        {
            throw new GraphQLException("No se ha podido identificar al usuario autenticado.");
        }

        return await _domiciliacionService.GetMyDomiciliaciones(userAuth);
    }
}