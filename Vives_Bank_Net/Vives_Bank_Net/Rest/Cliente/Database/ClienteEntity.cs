using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vives_Bank_Net.Rest.Cliente.Models;
using Vives_Bank_Net.Rest.User;

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

    [RegularExpression(@"^\d{8}[TRWAGMYFPDXBNJZSQVHLCKE]$", ErrorMessage = "El DNI debe tener 8 números seguidos de una letra en mayúsculas")]
    public string Dni { get; set; }

    
    public string Nombre { get; set; }

    public string Apellidos { get; set; }

    public Direccion Direccion { get; set; }
    
    public string Email { get; set; }
    
    public string Telefono { get; set; }
    
    [DefaultValue("https://example.com/fotoPerfil.jpg")]
    public string FotoPerfil { get; set; } = "https://example.com/fotoPerfil.jpg";
 
   [DefaultValue("https://example.com/fotoDni.jpg")]
   public string FotoDni { get; set; } = "https://example.com/fotoDni.jpg";


   // public ICollection<Cuenta.Models.Cuenta> Cuentas { get; set; } = new HashSet<Cuenta.Models.Cuenta>();
   
    //[Column("user_id")]
    // o al reves [ForeignKey("User")
   // public User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}