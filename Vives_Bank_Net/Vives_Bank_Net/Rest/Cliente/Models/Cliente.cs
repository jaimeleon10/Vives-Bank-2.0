using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vives_Banks_Net.Rest.User;
using Vives_Banks_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Cliente.Models;

[Table("Clientes")]
public class Cliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
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
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener 9 números")]
    public string Telefono { get; set; }

    [JsonPropertyName("fotopPerfil")]
    public string FotoPerfil { get; set; }

    [JsonPropertyName("fotoDni")]
    public string FotoDni { get; set; }

    public ICollection<Cuenta.Models.Cuenta> Cuentas { get; set; } = new HashSet<Cuenta.Models.Cuenta>();

    [JsonPropertyName("usuario")]
    public User User { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}