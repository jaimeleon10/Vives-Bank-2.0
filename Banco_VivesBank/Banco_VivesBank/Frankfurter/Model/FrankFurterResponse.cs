namespace Banco_VivesBank.Frankfurter.Model;

public class FrankFurterResponse
{
    public decimal Amount { get; set; }

    public string Base { get; set; }

    public string Date { get; set; }

    public Dictionary<string, decimal> Rates { get; set; }
    
}