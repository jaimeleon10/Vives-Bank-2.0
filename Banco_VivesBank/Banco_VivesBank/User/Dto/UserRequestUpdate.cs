using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequestUpdate
{
    [Required(ErrorMessage = "El campo de usuario es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre  de usuario no puede exceder los 50 caracteres.")]
    public string Username { get; set; }

    public string Role { get; set; } = Models.Role.User.GetType().ToString();

    public bool IsDeleted { get; set; } = false;
}