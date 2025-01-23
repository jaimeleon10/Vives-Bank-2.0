using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Base.Dto;

public class BaseRequest
{
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }
    
    [Required(ErrorMessage = "El campo descripcion es obligatorio")]
    [MaxLength(1000, ErrorMessage = "La descripcion no puede exceder los 1000 caracteres.")]
    public string Descripcion { get; set; }
    
    [Required(ErrorMessage = "El campo tipo es obligatorio")]
    [MaxLength(1000, ErrorMessage = "El tipo no puede exceder de los 1000 caracteres.")]
    public string TipoProducto { get; set; }
    
    [Required(ErrorMessage = "El campo TAE es obligatorio")]
    public double Tae { get; set; }
}

