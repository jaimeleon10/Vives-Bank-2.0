using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Database;

public class TarjetaEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;

    public string Guid { get; set; } = string.Empty;
    
    public string Numero { get; set; } = string.Empty;
    
    public string Titular { get; set; } = string.Empty;
    
    public string FechaVencimiento { get; set; } = string.Empty;
    
    public string Cvv { get; set; } = string.Empty;
    
    [Required]
    public string Pin { get; set; } = string.Empty;
    
    [Required]
    public double LimiteDiario { get; set; }
    
    [Required]
    public double LimiteSemanal { get; set; }
    
    [Required]
    public double LimiteMensual { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}