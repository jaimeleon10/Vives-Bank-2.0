using System.Text.Json;
using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Model;

namespace Banco_VivesBank.Frankfurter.Services;

public class DivisasService : IDivisasService
{
    private readonly HttpClient _httpClient;

    public DivisasService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
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