using System.Security.Cryptography;
using System.Text;

namespace Banco_VivesBank.Utils.Generators;

public class GuidGenerator
{
    public static string GenerarId()
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        using var rng = new RNGCryptoServiceProvider();

        var id = new StringBuilder(11);
        var buffer = new byte[1];

        for (int i = 0; i < 11; i++)
        {
            do
            {
                rng.GetBytes(buffer);
            } while (buffer[0] >= caracteres.Length * (256 / caracteres.Length));

            id.Append(caracteres[buffer[0] % caracteres.Length]);
        }

        return id.ToString();
    }
}