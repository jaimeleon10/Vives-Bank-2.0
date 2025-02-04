using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Banco_VivesBank.Utils.Validators;

namespace Banco_VivesBank.Movimientos.Dto;

public class IngresoNominaRequest
{
    [Required(ErrorMessage = "El nombre de la empresa en un campo obligatorio")]
    public string NombreEmpresa { get; set; }
    
    [Required(ErrorMessage = "El CIF de la empresa en un campo obligatorio")]
    public string CifEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanCliente { get; set; }
    
    [Required(ErrorMessage = "El importe es un campo obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El importe debe ser un número positivo")]
    public double Importe { get; set; }
}