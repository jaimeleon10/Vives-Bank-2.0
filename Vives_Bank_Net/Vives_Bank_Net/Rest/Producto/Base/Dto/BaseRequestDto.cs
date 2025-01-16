using System.ComponentModel.DataAnnotations;

namespace Vives_Bank_Net.Rest.Producto.Base.Dto;

public class BaseRequestDto
{
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre;
    
    [Required(ErrorMessage = "El campo descripcion es obligatorio")]
    [MaxLength(1000, ErrorMessage = "La descripcion no puede exceder los 1000 caracteres.")]
    public string Descripcion;
    
    [Required(ErrorMessage = "El campo TAE es obligatorio")]
    public double Tae;
}

