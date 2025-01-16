using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Vives_Bank_Net.Rest.Producto.Base.Database;

public class BaseEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;
    
    public string Guid { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000, ErrorMessage = "La descripcion no puede exceder los 1000 caracteres")]
    public string Descripcion { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100, ErrorMessage = "El tipo no puede estar vacio")]
    public string TipoProducto { get; set; } = string.Empty;
    
    [Required]
    public double Tae { get; set; } = 0.0;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}