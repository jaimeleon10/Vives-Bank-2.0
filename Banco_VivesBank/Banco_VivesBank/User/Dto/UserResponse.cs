namespace Banco_VivesBank.User.Dto;

public class UserResponse
{
    /// <summary>
    /// El identificador único del usuario.
    /// </summary>
    /// <example>Tgf27F</example>
    public string Guid { get; set; }
    /// <summary>
    /// El nombre de usuario 
    /// </summary>
    /// <example>JohnDoe</example>
    public string Username { get; set; }
    /// <summary>
    /// La contraseña del usuario
    /// </summary>
    /// <example>password</example>
    public string Password { get; set; }
    /// <summary>
    /// El rol del usuario
    /// </summary>
    /// <example>Admin</example> 
    public string Role { get; set; }
    /// <summary>
    /// La fecha de creación del usuario
    /// </summary>
    /// <example>2021-10-10</example>
    public string CreatedAt { get; set; }
    /// <summary>
    /// La fecha de actualización del usuario
    /// </summary>
    /// <example>2021-10-10</example>
    public string UpdatedAt { get; set; }
    /// <summary>
    /// Indica si el usuario está borrado
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; }
}