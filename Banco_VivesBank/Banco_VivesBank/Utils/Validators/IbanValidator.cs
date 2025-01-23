using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Banco_VivesBank.Utils.Validators;

public class IbanValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string iban = value as string;

        if (string.IsNullOrEmpty(iban))
        {
            return new ValidationResult(ErrorMessage ?? "El IBAN es un campo obligatorio");
        }

        if (!ValidateIban(iban))
        {
            return new ValidationResult(ErrorMessage ?? "El IBAN proporcionado no es válido");
        }

        return ValidationResult.Success;
    }

    private bool ValidateIban(string iban)
    {
        // Verifica que el IBAN tenga una longitud válida y cumpla con el patrón alfanumérico
        if (iban.Length < 15 || iban.Length > 34 || !Regex.IsMatch(iban, @"^[A-Z0-9]+$"))
        {
            return false;
        }

        // Reorganiza el IBAN moviendo los primeros 4 caracteres al final
        string reorganizedIban = iban.Substring(4) + iban.Substring(0, 4);

        // Convierte el IBAN reorganizado a una representación numérica
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

        // Calcula el módulo 97
        return Modulo97(numericIban.ToString()) == 1;
    }

    private int Modulo97(string number)
    {
        int remainder = 0;

        foreach (char digit in number)
        {
            remainder = (remainder * 10 + (digit - '0')) % 97;
        }

        return remainder;
    }
}