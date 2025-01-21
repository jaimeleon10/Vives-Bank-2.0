using System.Text;
using System.Text.RegularExpressions;

namespace Banco_VivesBank.Utils.Validators;

public static class IbanValidator
{
    public static class ValidarIban
    {
        public static bool ValidateIban(string iban)
        {
            // Comprueba que el IBAN no sea null, que tenga una longitud válida y que coincida con el patrón alfanumérico
            if (string.IsNullOrEmpty(iban) || iban.Length < 15 || iban.Length > 34 || !Regex.IsMatch(iban, @"^[A-Z0-9]+$"))
            {
                return false;
            }

            // Reorganiza el IBAN al mover los primeros 4 caracteres al final
            string reorganizedIban = iban.Substring(4) + iban.Substring(0, 4);

            // Convierte el IBAN reorganizado a una representación numérica según las reglas estándar
            StringBuilder numericIban = new StringBuilder();
            foreach (char c in reorganizedIban)
            {
                if (char.IsDigit(c))
                {
                    numericIban.Append(c);
                }
                else
                {
                    numericIban.Append(c - 'A' + 10);
                }
            }

            // Realiza el cálculo módulo 97 para validar el IBAN
            return Modulo97(numericIban.ToString()) == 1;
        }
        
        private static int Modulo97(string number)
        {
            int remainder = 0;
            
            foreach (char digit in number)
            {
                remainder = (remainder * 10 + (digit - '0')) % 97;
            }

            return remainder;
        }
    }
}