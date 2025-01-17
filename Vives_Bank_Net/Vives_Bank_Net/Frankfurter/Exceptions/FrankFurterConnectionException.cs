namespace Vives_Bank_Net.Frankfurter.Exceptions;

public class FrankFurterConnectionException : FrankFurterException
{
    public FrankFurterConnectionException(string monedaBase, string monedasObjetivo, Exception exception)
        : base($"Error de conexión al obtener las tasas de cambio de {monedaBase} a {monedasObjetivo}.", exception) { }
}