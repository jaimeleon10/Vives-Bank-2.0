using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.User.Dto;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteResponse
{
    /// <summary>
    /// Es un identificador único
    /// </summary>
    /// <example>1t2gVegRt2x</example> 
    public string Guid { get; set; }
    
    /// <summary>
    /// Dni del cliente
    /// </summary>
    /// <example>00000000T</example>
    public string Dni { get; set; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    /// <example>John</example>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Apellidos del cliente
    /// </summary>
    /// <example>Doe</example>
    public string Apellidos { get; set; }
    
    /// <summary>
    /// Direccion del cliente
    /// </summary>
    /// <example>
    ///{
    /// Calle = "Calle Falsa",
    /// Numero = "123",
    /// CodigoPostal = "12345",
    /// Piso = "1",
    /// Letra = "A"
    /// }
    /// </example>
    public Direccion Direccion  { get; set; }
    
    /// <summary>
    /// Email del cliente
    /// </summary>
    /// <example>email@example.com</example>
    public string Email { get; set; }
    
    /// <summary>
    /// Número de teléfono 
    /// </summary>
    /// <example>654126777</example>
    public string Telefono  { get; set; }
    
    /// <summary>
    /// Cadena con el nombre de la imagen de su perfil
    /// </summary>
    /// <example> avatar.jpg</example> 
    public string FotoPerfil  { get; set; }
    
    /// <summary>
    /// Cadena con el nombre de la imagen de su DNI
    /// </summary>
    /// <example>dniexample.jpg</example>
    public string FotoDni  { get; set; }
    
    /// <summary>
    /// Usuario asociado al cliente
    /// </summary>
    /// <example>
    /// {
    ///     Guid = "123456at",
    ///     Username = "JohnDoe",
    ///     Password = "password",
    ///     Role = "Cliente",
    ///     CreatedAt = "2021-10-10",
    ///     UpdatedAt = "2021-10-10",
    ///     IsDeleted = false
    /// }
    /// </example>
    public UserResponse UserResponse { get; set; }
    
    /// <summary>
    /// Fecha de creación del cliente
    /// </summary>
    /// <example>2021-10-10</example> 
    public string CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de la ultima actualización del cliente
    /// </summary>
    /// <example>2022-10-10</example>
    public string UpdatedAt { get; set; }

    /// <summary>
    /// Indica si el cliente está borrado
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; }
}