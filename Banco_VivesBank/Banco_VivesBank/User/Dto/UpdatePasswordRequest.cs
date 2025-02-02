using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UpdatePasswordRequest
{
    
    /// <summary>
    /// La contraseña debe ser obligatoria y tener entre 5 y 50 caracteres
    /// </summary>
    /// <example>password</example>
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string OldPassword { get; set; }
    /// <summary>
    /// La contraseña debe ser obligatoria y tener entre 5 y 50 caracteres y ser distinta a la anterior
    /// </summary>
    /// <example>password2</example>
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string NewPassword { get; set; }
    /// <summary>
    /// La contraseña debe ser obligatoria y tener entre 5 y 50 caracteres y ser igual a la anterior
    /// </summary>
    /// <example>password2</example> 
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string NewPasswordConfirmation { get; set; }
}