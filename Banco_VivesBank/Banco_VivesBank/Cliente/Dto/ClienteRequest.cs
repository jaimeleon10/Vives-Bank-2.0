using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Cliente.Dto;

/// <summary>
/// Representa la información necesaria para crear un nuevo cliente
/// </summary>
public class ClienteRequest
{
    /// <summary>
    /// El DNI del cliente, debe ser único y válido 
    /// </summary>
    /// <example>12345678Z</example>  
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [DniValidation]
    public string Dni { get; set; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    /// <example>John</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string Nombre { get; set; }

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    /// <example>Doe</example>
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [MaxLength(50, ErrorMessage = "Los apellidos debe tener como máximo 50 caracteres")]
    public string Apellidos { get; set; }

    /// <summary>
    /// Calle de la dirección
    /// </summary>
    /// <example>Calle Falsa</example>
    [Required(ErrorMessage = "La calle es obligatoria")]
    [MaxLength(150, ErrorMessage = "La calle debe tener como máximo 150 caracteres")]
    public string Calle { get; set; }
    
    /// <summary>
    /// Número de la calle
    /// </summary>
    /// <example>123</example>
    [Required(ErrorMessage = "El número de la dirección es obligatorio")]
    [MaxLength(5, ErrorMessage = "El numero de la direccion debe tener como máximo 4 caracteres")]
    public string Numero { get; set; }
    
    /// <summary>
    /// Código postal de la dirección
    /// </summary>
    /// <example>28000</example>
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string CodigoPostal { get; set; }
    
    /// <summary>
    /// Piso de la dirección
    /// </summary>
    /// <example>2</example>
    [Required(ErrorMessage = "El piso es obligatorio")]
    [MaxLength(3, ErrorMessage = "El piso debe tener como máximo 3 caracteres")]
    public string Piso { get; set; }
    
    /// <summary>
    /// Letra de la dirección
    /// </summary>
    /// <example>A</example>
    [Required(ErrorMessage = "La letra de la dirección es obligatorio")]
    [MaxLength(2, ErrorMessage = "La letra de la direccion debe tener como máximo 2 caracteres")]
    public string Letra { get; set; }
    
    /// <summary>
    /// Email del cliente
    /// </summary>
    /// <example>example@gmail.com</example>
    [Required(ErrorMessage = "El email es obligatorio")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe ser válido")]
    public string Email { get; set; }
    
    /// <summary>
    /// Número de teléfono del cliente
    /// </summary>
    /// <example>654111222</example>
    [Required(ErrorMessage = "El telefono es obligatorio")]
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string Telefono { get; set; }
    
    /// <summary>
    /// Este atributo es opcional, indica si el cliente ha sido borrado
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; } = false;
}