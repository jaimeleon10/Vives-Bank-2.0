using Vives_Bank_Net.Frankfurter.Model;

namespace Vives_Bank_Net.Frankfurter.Services;

public interface IDivisasService
{
    FrankFurterResponse ObtenerUltimasTasas(string monedaBase, string monedasObjetivo, string amount);
}