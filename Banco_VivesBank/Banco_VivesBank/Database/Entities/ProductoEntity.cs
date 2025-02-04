using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Banco_VivesBank.Database.Entities;

[Table("Productos") ]
public class ProductoEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;
    
    public string Guid { get; set; } = string.Empty;
    
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(250, ErrorMessage = "La descripcion no puede exceder los 250 caracteres")]
    public string Descripcion { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50, ErrorMessage = "El tipo de producto es un campo obligatorio y no puede exceder los 50 caracteres")]
    public string TipoProducto { get; set; } = string.Empty;
    
    [Required]
    public double Tae { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [System.ComponentModel.DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}