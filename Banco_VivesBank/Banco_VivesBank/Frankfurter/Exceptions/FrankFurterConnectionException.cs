namespace Banco_VivesBank.Frankfurter.Exceptions;

/// <summary>
/// Excepción personalizada que se lanza cuando ocurre un error de conexión al obtener las tasas de cambio entre divisas.
/// </summary>
public class FrankFurterConnectionException : FrankFurterException
{
    /// <summary>
    /// Constructor para inicializar una nueva instancia de la excepción `FrankFurterConnectionException`.
    /// </summary>
    /// <param name="monedaBase">La moneda base desde la cual se intentó obtener las tasas de cambio.</param>
    /// <param name="monedasObjetivo">Las monedas objetivo a las cuales se intentó obtener las tasas de cambio.</param>
    /// <param name="exception">La excepción interna que causó el error.</param>
    public FrankFurterConnectionException(string monedaBase, string monedasObjetivo, Exception exception)
        : base($"Error de conexión al obtener las tasas de cambio de {monedaBase} a {monedasObjetivo}.", exception) { }
}