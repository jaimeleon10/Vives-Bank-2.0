using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Tarjeta.Models;

namespace Banco_VivesBank.Database.Entities;

[Table("Cuentas")]
public class CuentaEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;
    
    [Required]
    public string Guid { get; set; }
    
    [Required]
    public string Iban { get; set; }
    
    [Required]
    public BigInteger Saldo { get; set; }
    
    [ForeignKey("Tarjeta")] 
    [Column("tarjeta_id")]
    public long? TarjetaId { get; set; }
    
    public Tarjeta? Tarjeta { get; set; }
    
    [ForeignKey("Cliente")] 
    [Column("cliente_id")]  
    [Required] 
    public long ClienteId { get; set; }
    public Cliente.Models.Cliente Cliente { get; set; } 

    
    [ForeignKey("Producto")] 
    [Column("producto_id")]  
    [Required] 
    public long ProductoId { get; set; }
    public BaseModel Producto { get; set; } 
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }
}