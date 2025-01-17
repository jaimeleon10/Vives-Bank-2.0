namespace Vives_Bank_Net.Frankfurter.Exceptions;

public class FrankfurterEmptyResponseException : FrankFurterException
{
    public FrankfurterEmptyResponseException(string monedaBase, string monedasObjetivo, string cantidad)
        : base($"No se obtuvieron datos en la respuesta de FrankFurter para la moneda '{monedaBase}', símbolo '{monedasObjetivo}' y cantidad '{cantidad}'.") { }
}