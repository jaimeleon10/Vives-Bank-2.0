using GraphQL;
using GraphQL.Types;
using Vives_Bank_Net.Rest.Movimientos.Services;

namespace Banco_VivesBank.GraphQL;

public sealed class MovimientoQuery : ObjectGraphType
{
    private readonly IMovimientoService _movimientoService;
    public MovimientoQuery(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;

        Field<ListGraphType<MovimientoType>>("movimientos")
            .Resolve(context =>
            {
                var movimientos = _movimientoService.GetAllAsync();
                return movimientos;
            });

        Field<MovimientoType>("movimiento")
            .Arguments(new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" }
            )).Resolve(context =>
            {
                var id = context.GetArgument<string>("id");
                var movimiento = _movimientoService.GetByIdAsync(id);
                return movimiento;
            });
    }
}