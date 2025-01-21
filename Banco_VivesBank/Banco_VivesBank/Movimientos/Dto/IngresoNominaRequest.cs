using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class IngresoNominaRequest
{
    [IbanValidator]
    public string IbanOrigen { get; set; }
    
    [IbanValidator]
    public string IbanDestino { get; set; }
    
    [BigIntegerValidation]
    public BigInteger Importe { get; set; }
    
    [Required(ErrorMessage = "El nombre de la empresa en un campo obligatorio")]
    public string NombreEmpresa { get; set; }
    
    [Required(ErrorMessage = "El CIF de la empresa en un campo obligatorio")]
    public string CifEmpresa { get; set; }
}