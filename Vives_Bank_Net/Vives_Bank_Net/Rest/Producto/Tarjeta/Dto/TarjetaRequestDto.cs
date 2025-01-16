using System.ComponentModel.DataAnnotations;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Dto;

public class TarjetaRequestDto
{
    [Required(ErrorMessage = "Es necesario definir un Pin")]
    [MaxLength(4, ErrorMessage = "El pin debe tener una longitud de 4 caracteres")]
    public string Pin;

    [Required(ErrorMessage = "Debes establecer un limite diario superior a 0")]
    public double LimiteDiario;
    
    [Required(ErrorMessage = "Debes establecer un limite semanal superior a 0 y al limite diario")]
    public double LimiteSemanal;

    [Required(ErrorMessage = "Debes establecer un limite mensual superior a 0 y al limite semanal")]
    public double LimiteMensual;

}