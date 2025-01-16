namespace Vives_Bank_Net.Frankfurter.Exceptions;

public class FrankFurterUnexpectedException : FrankFurterException
{
    public FrankFurterUnexpectedException(string monedaBase, string monedasObjetivo)
        : base($"Error inesperado al obtener las tasas de cambio de {monedaBase} a {monedasObjetivo}.") { }
}