using System.Numerics;

namespace Banco_VivesBank.Utils.Generators;

public class IbanGenerator
{
    public static string GenerateIban()
    {
        string countryCode = "ES";
        string bankCode = "1234";
        string branchCode = "1234";
        string controlDigits = new Random().Next(0, 100).ToString("D2");
        string accountNumber = new Random().Next(0, 1_000_000_000).ToString("D10");

        // Construimos un IBAN temporal para calcular los dígitos de verificación
        string tempIban = bankCode + branchCode + controlDigits + accountNumber + "142800";

        // Convertimos el IBAN temporal a su representación numérica
        string numericIban = string.Concat(tempIban.Select(c =>
            char.IsDigit(c) ? c.ToString() : (c - 'A' + 10).ToString()));

        // Calculamos el checksum según el método módulo 97
        BigInteger numericValue = BigInteger.Parse(numericIban);
        int checksum = 98 - (int)(numericValue % 97);

        // Devolvemos el IBAN final
        return $"{countryCode}{checksum:D2}{bankCode}{branchCode}{controlDigits}{accountNumber}";
    }
}