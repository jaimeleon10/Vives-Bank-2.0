namespace Banco_VivesBank.Frankfurter.Model;

/// <summary>
/// Representa la respuesta recibida al consultar las tasas de cambio desde una moneda base a varias monedas objetivo.
/// </summary>
public class FrankFurterResponse
{
    /// <summary>
    /// La cantidad solicitada para la conversión de monedas.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// La moneda base desde la cual se realiza la conversión.
    /// </summary>
    public string Base { get; set; }
    
    /// <summary>
    /// Un diccionario que contiene las tasas de cambio para cada moneda objetivo.
    /// Las claves del diccionario son los símbolos de las monedas objetivo,
    /// y los valores son las tasas de cambio correspondientes.
    /// </summary>
    public Dictionary<string, decimal> Rates { get; set; }
    
}