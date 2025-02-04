using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Banco_VivesBank.Utils.Validators;

public class DniValidation : ValidationAttribute
{
    private const string DniPattern = @"^\d{8}[TRWAGMYFPDXBNJZSQVHLCKE]$";
    private const string LetrasValidas = "TRWAGMYFPDXBNJZSQVHLCKE";


    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success!;
        }
        var dni = value.ToString().ToUpper();
        
        if (!Regex.IsMatch(dni, DniPattern))
        {
            return new ValidationResult("El DNI debe tener 8 números seguidos de una letra en mayúsculas");
        }

        var numeros = int.Parse(dni.Substring(0, 8));
        var letra = dni[8];
        
        if (LetrasValidas[numeros % 23] != letra)
        {
            return new ValidationResult("La letra del DNI no es correcta");
        }

        return ValidationResult.Success!;
    }
}