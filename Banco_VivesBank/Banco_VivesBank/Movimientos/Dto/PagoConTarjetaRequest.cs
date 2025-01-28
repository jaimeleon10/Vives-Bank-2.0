using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class PagoConTarjetaRequest
{
    [Required(ErrorMessage = "El nombre del comercio es un campo obligatorio")]
    public string NombreComercio { get; set; }
    
    [Required(ErrorMessage = "El importe es un campo obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El importe debe ser un número positivo")]
    public double Importe { get; set; }
    
    [CreditCardValidation]
    public string NumeroTarjeta { get; set; }
}