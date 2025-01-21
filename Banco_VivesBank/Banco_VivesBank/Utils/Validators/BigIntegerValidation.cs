using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Banco_VivesBank.Utils.Validators;

public class BigIntegerValidation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is BigInteger bigIntegerValue)
        {
            if (bigIntegerValue > 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage ?? "El valor debe ser mayor a cero");
            }
        }

        return new ValidationResult(ErrorMessage ?? "El valor no es válido, debe ser de tipo BigInteger");
    }
}