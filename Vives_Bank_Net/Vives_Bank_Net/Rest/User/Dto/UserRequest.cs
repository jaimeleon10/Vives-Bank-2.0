using System.ComponentModel.DataAnnotations;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Rest.User.Dto;

public class UserRequest
{
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; } = null!;
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [MinLength(5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres")]
    public string Password { get; set; } = null!;

    public string Role { get; set; } = Models.Role.USER.GetType().ToString();

    public bool IsDeleted { get; set; } = false;

}