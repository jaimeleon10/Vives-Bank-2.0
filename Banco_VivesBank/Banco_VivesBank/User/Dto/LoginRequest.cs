namespace Banco_VivesBank.User.Dto;
/// <summary>
/// Representa la clase LoginRequest con los atributos Username y Password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Obtiene o establece el nombre de usuario del usuario.
    /// </summary>
    /// <example>JohnDoe</example>  
    public string Username { get; set; }
    /// <summary>
    /// Obtiene o establece la contraseña del usuario.
    /// </summary>
    /// <example>password</example>
    public string Password { get; set; }
}