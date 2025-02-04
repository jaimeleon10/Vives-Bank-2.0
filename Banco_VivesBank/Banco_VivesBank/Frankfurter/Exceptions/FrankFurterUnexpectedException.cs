namespace Banco_VivesBank.Frankfurter.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando ocurre un error inesperado al obtener las tasas de cambio.
/// </summary>
public class FrankFurterUnexpectedException : FrankFurterException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase `FrankFurterUnexpectedException` con un mensaje de error 
    /// que describe un error inesperado al obtener las tasas de cambio entre dos monedas.
    /// </summary>
    /// <param name="monedaBase">La moneda base desde la cual se obtienen las tasas de cambio.</param>
    /// <param name="monedasObjetivo">La moneda o monedas objetivo para las cuales se obtienen las tasas de cambio.</param>
    public FrankFurterUnexpectedException(string monedaBase, string monedasObjetivo)
        : base($"Error inesperado al obtener las tasas de cambio de {monedaBase} a {monedasObjetivo}.") { }
}