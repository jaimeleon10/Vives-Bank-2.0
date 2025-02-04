namespace Banco_VivesBank.Frankfurter.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando la respuesta de FrankFurter no contiene datos válidos.
/// </summary>
public class FrankfurterEmptyResponseException : FrankFurterException
{
    /// <summary>
    /// Constructor para inicializar una nueva instancia de la excepción `FrankfurterEmptyResponseException`.
    /// </summary>
    /// <param name="monedaBase">La moneda base desde la cual se intentó obtener las tasas de cambio.</param>
    /// <param name="monedasObjetivo">Las monedas objetivo a las cuales se intentó obtener las tasas de cambio.</param>
    /// <param name="cantidad">La cantidad solicitada para obtener la tasa de cambio.</param>
    public FrankfurterEmptyResponseException(string monedaBase, string monedasObjetivo, string cantidad)
        : base($"No se obtuvieron datos en la respuesta de FrankFurter para la moneda '{monedaBase}', símbolo '{monedasObjetivo}' y cantidad '{cantidad}'.") { }
}