using Banco_VivesBank.Frankfurter.Model;

namespace Banco_VivesBank.Frankfurter.Services;

/// <summary>
/// Interfaz para el servicio que obtiene las últimas tasas de cambio entre una moneda base y monedas objetivo.
/// </summary>
public interface IDivisasService
{
    /// <summary>
    /// Obtiene las últimas tasas de cambio entre una moneda base y una o varias monedas objetivo.
    /// </summary>
    /// <param name="monedaBase">La moneda base desde la cual se realiza la conversión (ej. "EUR").</param>
    /// <param name="monedasObjetivo">Las monedas objetivo a las cuales se desea convertir (ej. "USD,GBP").</param>
    /// <param name="amount">La cantidad a convertir (ej. "100"). Si no se especifica, se usa el valor por defecto "1".</param>
    /// <returns>Un objeto de tipo <see cref="FrankFurterResponse"/> con las tasas de cambio obtenidas.</returns>
    FrankFurterResponse ObtenerUltimasTasas(string monedaBase, string monedasObjetivo, string amount);
}