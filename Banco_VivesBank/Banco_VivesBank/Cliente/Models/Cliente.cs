using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Cliente.Models;

public class Cliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [Required]
    [RegularExpression(@"^\d{8}[TRWAGMYFPDXBNJZSQVHLCKE]$", ErrorMessage = "El DNI debe tener 8 números seguidos de una letra en mayúsculas")]
    public string Dni { get; set; }

    [Required]
    public string Nombre { get; set; }

    [Required]
    public string Apellidos { get; set; }
    
    [Required]
    public Direccion Direccion { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe ser válido")]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string Telefono { get; set; }
    
    [JsonPropertyName("fotopPerfil")]
    public string FotoPerfil { get; set; } = "https://example.com/fotoPerfil.jpg";

    [JsonPropertyName("fotoDni")]
    public string FotoDni { get; set; } = "https://example.com/fotoDni.jpg";

    [JsonPropertyName("cuentas")]
    public ICollection<Cuenta> Cuentas { get; set; } = new HashSet<Cuenta>();

    [JsonPropertyName("usuario")]
    public User.Models.User User { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}