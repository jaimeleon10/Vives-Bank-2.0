using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteRequestUpdate
{
    [DniValidation]
    public string? Dni { get; set; }
    
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string? Nombre { get; set; }

    [MaxLength(50, ErrorMessage = "Los apellidos debe tener como máximo 50 caracteres")]
    public string? Apellidos { get; set; }

    [MaxLength(150, ErrorMessage = "La calle debe tener como máximo 150 caracteres")]
    public string? Calle { get; set; }
    
    [MaxLength(5, ErrorMessage = "El numero de la direccion debe tener como máximo 4 caracteres")]
    public string? Numero { get; set; }
    
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string? CodigoPostal { get; set; }
    
    [MaxLength(3, ErrorMessage = "El piso debe tener como máximo 3 caracteres")]
    public string? Piso { get; set; }
    
    [MaxLength(2, ErrorMessage = "La letra de la direccion debe tener como máximo 2 caracteres")]
    public string? Letra { get; set; }
    
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe ser válido")]
    public string? Email { get; set; }
    
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string? Telefono { get; set; }
    
    
    public bool HasAtLeastOneField()
    {
        return new[] { Nombre, Apellidos, Calle, Numero, CodigoPostal, Piso, Letra, Email, Telefono }
            .Any(field => !string.IsNullOrWhiteSpace(field));
    }
}