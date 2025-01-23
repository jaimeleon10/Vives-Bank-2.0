using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class IngresoNominaRequest
{
    [Required(ErrorMessage = "El nombre de la empresa en un campo obligatorio")]
    public string NombreEmpresa { get; set; }
    
    [Required(ErrorMessage = "El CIF de la empresa en un campo obligatorio")] // TODO -> HACER CifValidator
    public string CifEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanCliente { get; set; }
    
    [Required(ErrorMessage = "El importe es un campo obligatorio.")]
    [RegularExpression(@"^-?\d+(\.\d{1,2})?$", ErrorMessage = "El importe debe ser un número entero válido.")]
    public string Importe { get; set; }
}