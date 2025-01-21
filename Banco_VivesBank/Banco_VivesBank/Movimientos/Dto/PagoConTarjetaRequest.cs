using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class PagoConTarjetaRequest
{
    [CreditCardValidation]
    public string NumeroTarjeta { get; set; }
    
    [BigIntegerValidation]
    public BigInteger Importe { get; set; }
    
    [Required(ErrorMessage = "El nombre del comercio es un campo obligatorio")]
    public string NombreComercio { get; set; }
}