using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteRequestSave
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [ValidationDNI(ErrorMessage = "El DNI no es válido")]
    public string Dni { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string Nombre { get; set; } = null!;
    
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [MinLength(5, ErrorMessage = "Los apellidos debe tener menos 5 caracteres")]
    [MaxLength(255, ErrorMessage = "Los apellidos debe tener como máximo 255 caracteres")]
    public string Apellidos { get; set; } = null!;
    
    [Required(ErrorMessage = "La calle es obligatoria")]
    [MaxLength(255, ErrorMessage = "La calle debe tener como máximo 255 caracteres")]
    public string Calle { get; set; } = null!;
    
    [Required(ErrorMessage = "El número de la dirección es obligatorio")]
    [MaxLength(25, ErrorMessage = "El numero de la direccion debe tener como máximo 25 caracteres")]
    public string Numero { get; set; } = null!;
    
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string CodigoPostal { get; set; } = null!;
    
    [Required(ErrorMessage = "El piso es obligatorio")]
    [MaxLength(50, ErrorMessage = "El piso debe tener como máximo 255 caracteres")]
    public string Piso { get; set; } = null!;
    
    [Required(ErrorMessage = "La letra de la dirección es obligatorio")]
    [MaxLength(25, ErrorMessage = "La letra de la direccion debe tener como máximo 25 caracteres")]
    public string Letra { get; set; } = null!;
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Debe ingresar un email correcto")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "El telefono es obligatorio")]
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string Telefono { get; set; } = null!;
}