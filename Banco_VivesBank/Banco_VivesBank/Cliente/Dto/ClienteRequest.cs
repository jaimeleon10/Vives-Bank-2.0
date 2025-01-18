using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteRequest
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [RegularExpression(@"^\d{8}[TRWAGMYFPDXBNJZSQVHLCKE]$", ErrorMessage = "El DNI debe tener 8 números seguidos de una letra en mayúsculas")]
    public string Dni { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [MaxLength(50, ErrorMessage = "Los apellidos debe tener como máximo 50 caracteres")]
    public string Apellidos { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria")]
    [MaxLength(150, ErrorMessage = "La calle debe tener como máximo 150 caracteres")]
    public string Calle { get; set; }
    
    [Required(ErrorMessage = "El número de la dirección es obligatorio")]
    [MaxLength(5, ErrorMessage = "El numero de la direccion debe tener como máximo 4 caracteres")]
    public string Numero { get; set; }
    
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string CodigoPostal { get; set; }
    
    [Required(ErrorMessage = "El piso es obligatorio")]
    [MaxLength(3, ErrorMessage = "El piso debe tener como máximo 3 caracteres")]
    public string Piso { get; set; }
    
    [Required(ErrorMessage = "La letra de la dirección es obligatorio")]
    [MaxLength(2, ErrorMessage = "La letra de la direccion debe tener como máximo 2 caracteres")]
    public string Letra { get; set; }
    
    [Required(ErrorMessage = "El email es obligatorio")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe ser válido")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "El telefono es obligatorio")]
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string Telefono { get; set; }
    
    [Required(ErrorMessage = "El guid del usuario es obligatorio")]
    public string UserGuid { get; set; }
    
    public bool IsDeleted { get; set; } = false;
}