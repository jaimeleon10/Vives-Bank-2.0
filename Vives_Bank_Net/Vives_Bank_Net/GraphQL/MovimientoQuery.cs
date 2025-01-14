
using GraphQL.Types;

namespace Vives_Bank_Net.GraphQL;

public class MovimientoQuery : ObjectGraphType
{
    public MovimientoQuery()
    {
        Field<ListGraphType<MovimientoType>>("movimientos")
            .Resolve(context => null); // TODO: CAMBIAR NULL POR GETALL DE MOVIMIENTO
    }
}