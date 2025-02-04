using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Microsoft.EntityFrameworkCore;

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
    [MaxLength(50, ErrorMessage = "El nombre debe tener como máximo 50 caracteres")]
    public string Nombre { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "Los apellidos debe tener como máximo 50 caracteres")]
    public string Apellidos { get; set; }

    [Required]
    public Direccion Direccion { get; set; }
    
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe ser válido")]
    public string Email { get; set; }
    
    [Required]
    [RegularExpression(@"^[679]\d{8}$", ErrorMessage = "Debe ingresar un teléfono válido.")]
    public string Telefono { get; set; }
    
    [System.ComponentModel.DefaultValue("https://example.com/fotoPerfil.jpg")]
    public string FotoPerfil { get; set; } = "https://example.com/fotoPerfil.jpg";
 
    [System.ComponentModel.DefaultValue("https://example.com/fotoDni.jpg")]
    public string FotoDni { get; set; } = "https://example.com/fotoDni.jpg";
    
    public ICollection<CuentaEntity> Cuentas { get; set; } = new HashSet<CuentaEntity>();
   
    [ForeignKey("User")]
    [Column("user_id")]
    public long UserId { get; set; }
    public UserEntity User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [System.ComponentModel.DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}