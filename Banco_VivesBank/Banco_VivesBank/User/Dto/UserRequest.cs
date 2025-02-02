using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequest
{
    /// <summary>
    /// El nombre de usuario debe ser obligatorio y tener entre 5 y 50 caracteres
    /// </summary>
    /// <example>JohnDoe</example>
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; }
    /// <summary>
    /// La contraseña debe ser obligatoria y tener entre 5 y 50 caracteres
    /// </summary>
    /// <example>password</example>
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string Password { get; set; }
    /// <summary>
    /// La contraseña debe ser obligatoria y tener entre 5 y 50 caracteres y ser igual a la anterior
    /// </summary>
    /// <example>password</example>
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string PasswordConfirmation { get; set; }
}