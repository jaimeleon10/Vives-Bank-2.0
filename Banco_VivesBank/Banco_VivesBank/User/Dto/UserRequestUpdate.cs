using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequestUpdate
{
    /// <summary>
    /// Obtiene o establece el nombre de usuario del usuario.
    /// </summary>
    /// <example>JohnDoe</example>
    public string Role { get; set; } = Models.Role.User.GetType().ToString();
    /// <summary>
    /// Obtiene o establece el estado de eliminación del usuario.
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; } = false;
}