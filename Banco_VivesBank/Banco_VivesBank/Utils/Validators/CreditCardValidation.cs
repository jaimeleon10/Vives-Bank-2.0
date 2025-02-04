using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Utils.Validators;

public class CreditCardValidation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string cardNumber = value as string;

        if (string.IsNullOrEmpty(cardNumber))
        {
            return new ValidationResult(ErrorMessage ?? "El número de tarjeta es un campo obligatorio");
        }

        if (!IsLuhnValid(cardNumber))
        {
            return new ValidationResult(ErrorMessage ?? "El número de tarjeta no es válido");
        }

        return ValidationResult.Success;
    }

    private bool IsLuhnValid(string cardNumber)
    {
        int suma = 0;
        bool duplicar = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(cardNumber[i]))
            {
                return false;
            }

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

        return suma % 10 == 0;
    }
}