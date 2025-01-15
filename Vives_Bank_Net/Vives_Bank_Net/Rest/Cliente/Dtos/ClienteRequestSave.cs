using System.ComponentModel.DataAnnotations;

namespace Vives_Bank_Net.Rest.Cliente.Dtos;

public class ClienteRequestSave
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(9, ErrorMessage = "El DNI debe tener 9 caracteres")]
    public string Dni { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string Nombre { get; set; } = null!;
    
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [MinLength(5, ErrorMessage = "Los apellidos debe tener menos 5 caracteres")]
    [MaxLength(255, ErrorMessage = "Los apellidos debe tener como máximo 255 caracteres")]
    public string Apellidos { get; set; } = null!;
    
    [Required(ErrorMessage = "La calle es obligatoria")]
    public string Calle { get; set; } = null!;
    
    [Required(ErrorMessage = "El número de la dirección es obligatorio")]
    public string Numero { get; set; } = null!;
    
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [StringLength(5, ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string CodigoPostal { get; set; } = null!;
    
    [Required(ErrorMessage = "El piso es obligatorio")]
    public string Piso { get; set; } = null!;
    
    [Required(ErrorMessage = "La letra de la dirección es obligatorio")]
    public string Letra { get; set; } = null!;
    
    [Required(ErrorMessage = "El email es obligatorio")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "El telefono es obligatorio")]
    public string Telefono { get; set; } = null!;
    
}