using System.Text;

namespace Banco_VivesBank.Utils.Generators;

public class TarjetaGenerator
{
    private static readonly Random Random = new Random();

    public static string GenerarTarjeta()
    {
        int length = 16; // Longitud fija para tarjetas en España
        StringBuilder cardNumber = new StringBuilder();

        // Genera los primeros 15 dígitos aleatorios
        for (int i = 0; i < length - 1; i++)
        {
            cardNumber.Append(Random.Next(0, 10));
        }

        // Calcula el dígito de control (checksum) usando Luhn
        int checksum = CalculateLuhn(cardNumber.ToString());
        cardNumber.Append(checksum);

        return cardNumber.ToString();
    }

    private static int CalculateLuhn(string partialCardNumber)
    {
        int suma = 0;
        bool duplicar = true;

        for (int i = partialCardNumber.Length - 1; i >= 0; i--)
        {
            int digit = partialCardNumber[i] - '0';

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

        int checksum = (10 - (suma % 10)) % 10;
        return checksum;
    }
}