using System.Text;

namespace Banco_VivesBank.Utils.Generators;

public class TarjetaGenerator
{
    public static string GenerarTarjeta()
    {
        var numTarjeta = new StringBuilder();
        numTarjeta.Append("4");
        var random = new Random();

        for (int i = 0; i < 13; i++)
        {
            numTarjeta.Append(random.Next(10));
        }

        int digitoDeControl = CalculoLuhn(numTarjeta.ToString());
        numTarjeta.Append(digitoDeControl);

        return numTarjeta.ToString();
    }

    private static int CalculoLuhn(string cardNumber)
    {
        int suma = 0;
        bool duplicar = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

            if (duplicar)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            suma += digit;
            duplicar = !duplicar;
        }

        return (10 - (suma % 10)) % 10;
    }
}