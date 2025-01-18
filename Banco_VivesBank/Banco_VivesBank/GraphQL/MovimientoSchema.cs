using GraphQL.Types;

namespace Banco_VivesBank.GraphQL;

public class MovimientoSchema : Schema
{
    public MovimientoSchema(IServiceProvider provider) : base(provider)
    {
        Query = provider.GetRequiredService<MovimientoQuery>();
    }
}