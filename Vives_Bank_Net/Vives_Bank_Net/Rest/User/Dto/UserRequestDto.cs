using System.ComponentModel.DataAnnotations;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Rest.User.Dto;

public class UserRequestDto
{
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; } = null!;
    
    
    [Required(ErrorMessage = "El campo de contraseña es obligatorio")]
    [MinLength(5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres")]
    public string PasswordHash { get; set; } = null!;
    
    [Required(ErrorMessage = "El campo de rol es obligatorio")]
    public Role Role { get; set; }
    
    
    [Required(ErrorMessage = "El campo isDeleted es obligatorio")]
    public bool IsDeleted { get; set; } = false;
    
}