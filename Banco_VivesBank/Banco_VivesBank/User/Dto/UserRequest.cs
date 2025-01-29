using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequest
{
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string PasswordConfirmation { get; set; }
}