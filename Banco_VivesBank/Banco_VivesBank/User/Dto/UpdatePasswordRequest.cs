using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UpdatePasswordRequest
{
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string OldPassword { get; set; }
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d\S]{5,50}$", ErrorMessage = "La contraseña debe tener un número, una mayúscula, una minúscula y entre 5 y 50 caracteres")]
    public string NewPasswordConfirmation { get; set; }
}