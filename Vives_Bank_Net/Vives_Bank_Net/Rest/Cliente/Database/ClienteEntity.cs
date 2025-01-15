using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StackExchange.Redis;
using Vives_Bank_Net.Rest.Cliente.Models;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Cliente.Database;

[Table("Clientes") ]
public class ClienteEntity
{

    public const long NewId = 0; 
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;

    [Required]
    public string Guid { get; set; }

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

    [Required]
    public string FotoPerfil { get; set; }

    [Required]
    public string FotoDni { get; set; }

    public ICollection<Cuenta.Models.Cuenta> Cuentas { get; set; } = new HashSet<Cuenta.Models.Cuenta>();

    [Required]
    public User.Models.User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}