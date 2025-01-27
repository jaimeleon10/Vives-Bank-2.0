using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.ProductoBase.Dto;

public class ProductoRequestUpdate
{
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }
    
    [Required(ErrorMessage = "El campo descripcion es obligatorio")]
    [MaxLength(1000, ErrorMessage = "La descripcion no puede exceder los 1000 caracteres.")]
    public string Descripcion { get; set; }
    
    [Required(ErrorMessage = "El campo TAE es obligatorio")]
    public double Tae { get; set; }
    
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}