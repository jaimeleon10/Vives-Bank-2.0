using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.Cliente.Models;

namespace Banco_VivesBank.Database.Entities;

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
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Nombre { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres")]
    public string Apellidos { get; set; }

    [Required]
    public Direccion Direccion { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Telefono { get; set; }
    
    [DefaultValue("https://example.com/fotoPerfil.jpg")]
    public string FotoPerfil { get; set; } = "https://example.com/fotoPerfil.jpg";
 
    [DefaultValue("https://example.com/fotoDni.jpg")]
    public string FotoDni { get; set; } = "https://example.com/fotoDni.jpg";
    
   // public ICollection<Cuenta.Models.Cuenta> Cuentas { get; set; } = new HashSet<Cuenta.Models.Cuenta>();
   
    //[Column("user_id")]
    // o al reves [ForeignKey("User")
   // public User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}