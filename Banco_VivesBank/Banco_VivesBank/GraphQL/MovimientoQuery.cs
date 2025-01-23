using Banco_VivesBank.Movimientos.Services;
using GraphQL;
using GraphQL.Types;

namespace Banco_VivesBank.GraphQL;

public sealed class MovimientoQuery : ObjectGraphType
{
    private readonly IMovimientoService _movimientoService;
    public MovimientoQuery(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;

        Field<ListGraphType<MovimientoType>>("movimientos")
            .ResolveAsync(async context =>
            {
                var movimientos = await _movimientoService.GetAllAsync();
                return movimientos;
            });

        Field<MovimientoType>("movimientoByGuid")
            .Arguments(new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "guid" }
            )).ResolveAsync(async context =>
            {
                var guid = context.GetArgument<string>("guid");
                var movimiento = await _movimientoService.GetByGuidAsync(guid);
                return movimiento;
            });
        
        Field<MovimientoType>("movimientoByClienteGuid")
            .Arguments(new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "clienteGuid" }
            )).ResolveAsync(async context =>
            {
                var clienteGuid = context.GetArgument<string>("clienteGuid");
                var movimiento = await _movimientoService.GetByClienteGuidAsync(clienteGuid);
                return movimiento;
            });
    }
}