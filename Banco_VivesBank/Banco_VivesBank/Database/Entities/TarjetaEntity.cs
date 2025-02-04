using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Database.Entities;

[Table("Tarjetas")]
public class TarjetaEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;

    [Required]
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [Required]
    public string Numero { get; set; } = TarjetaGenerator.GenerarTarjeta();

    [Required]
    public string FechaVencimiento { get; set; } = ExpDateGenerator.GenerarExpDate();

    [Required]
    public string Cvv { get; set; } = CvvGenerator.GenerarCvv();
    
    [Required]
    public string Pin { get; set; }
    
    [Required]
    public double LimiteDiario { get; set; }
    
    [Required]
    public double LimiteSemanal { get; set; }
    
    [Required]
    public double LimiteMensual { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [System.ComponentModel.DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}