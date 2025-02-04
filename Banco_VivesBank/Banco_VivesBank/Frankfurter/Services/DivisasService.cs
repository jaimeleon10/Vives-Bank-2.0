using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Model;
using System.Text.Json;

namespace Banco_VivesBank.Frankfurter.Services;

/// <summary>
/// Servicio encargado de obtener las últimas tasas de cambio entre una moneda base y monedas objetivo desde la API de Frankfurter.
/// </summary>
public class DivisasService : IDivisasService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Constructor que inicializa el servicio con un HttpClient.
    /// </summary>
    /// <param name="httpClient">Instancia de HttpClient para realizar las solicitudes HTTP.</param>
    public DivisasService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Obtiene las últimas tasas de cambio entre una moneda base y una o varias monedas objetivo.
    /// </summary>
    /// <param name="monedaBase">La moneda base desde la cual se realiza la conversión (ej. "EUR").</param>
    /// <param name="monedasObjetivo">Las monedas objetivo a las cuales se desea convertir (ej. "USD,GBP").</param>
    /// <param name="amount">La cantidad a convertir (ej. "100"). Si no se especifica, se usa el valor por defecto "1".</param>
    /// <returns>Un objeto de tipo <see cref="FrankFurterResponse"/> con las tasas de cambio obtenidas.</returns>
    /// <exception cref="FrankFurterUnexpectedException">Lanzado si la API no responde correctamente.</exception>
    /// <exception cref="FrankfurterEmptyResponseException">Lanzado si la respuesta de la API está vacía o no contiene las tasas de cambio.</exception>
    public FrankFurterResponse ObtenerUltimasTasas(string monedaBase, string monedasObjetivo, string amount)
    {
        amount = string.IsNullOrWhiteSpace(amount) ? "1" : amount;
        var url = $"https://api.frankfurter.app/latest?base={monedaBase}&symbols={monedasObjetivo}&amount={amount}";

        var response = _httpClient.GetAsync(url).Result;

        if (!response.IsSuccessStatusCode)
        {
            throw new FrankFurterUnexpectedException(monedaBase, monedasObjetivo);
        }

        var jsonString = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<FrankFurterResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null || result.Rates == null || !result.Rates.Any())
        {
            throw new FrankfurterEmptyResponseException(monedaBase, monedasObjetivo, amount);
        }


        return result;
    }
}