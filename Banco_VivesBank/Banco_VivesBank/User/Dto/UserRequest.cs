using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequest
{
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [MinLength(5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres")]
    public string Password { get; set; }

    public string Role { get; set; } = Models.Role.User.GetType().ToString();

    public bool IsDeleted { get; set; } = false;
}