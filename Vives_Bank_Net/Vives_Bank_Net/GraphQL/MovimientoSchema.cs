using GraphQL.Types;

namespace Vives_Bank_Net.GraphQL;

public class MovimientoSchema : Schema
{
    public MovimientoSchema(IServiceProvider provider) : base(provider)
    {
        Query = provider.GetRequiredService<MovimientoQuery>();
    }
}