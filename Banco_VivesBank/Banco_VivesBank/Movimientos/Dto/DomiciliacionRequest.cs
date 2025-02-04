using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;
using Renci.SshNet.Common;
using BigInteger = System.Numerics.BigInteger;

namespace Banco_VivesBank.Movimientos.Dto;

public class DomiciliacionRequest
{
    [Required(ErrorMessage = "El acreedor es un campo obligatorio")]
    public string Acreedor { get; set; }
    
    [IbanValidator]
    public string IbanEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanCliente { get; set; }
    
    [Required(ErrorMessage = "El importe es un campo obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El importe debe ser un número positivo")]
    public double Importe { get; set; }
    
    
    public string Periodicidad { get; set; } = Models.Periodicidad.Semanal.GetType().ToString();

    public bool Activa { get; set; } = true;
}