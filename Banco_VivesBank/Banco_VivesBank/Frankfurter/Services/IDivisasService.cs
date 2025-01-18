using Banco_VivesBank.Frankfurter.Model;

namespace Banco_VivesBank.Frankfurter.Services;

public interface IDivisasService
{
    FrankFurterResponse ObtenerUltimasTasas(string monedaBase, string monedasObjetivo, string amount);
}