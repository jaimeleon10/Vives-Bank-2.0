using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;
using Renci.SshNet.Common;
using BigInteger = System.Numerics.BigInteger;

namespace Banco_VivesBank.Movimientos.Dto;

public class DomiciliacionRequest
{
    [Required(ErrorMessage = "El guid del cliente es un campo obligatorio")]
    public string ClienteGuid { get; set; }
    
    [IbanValidator]
    public string IbanEmpresa { get; set; }
    
    [IbanValidator]
    public string IbanCliente { get; set; }
    
    [Required(ErrorMessage = "El importe es obligatorio")]
    [BigIntegerValidation]
    public BigInteger Importe { get; set; }
    
    [Required(ErrorMessage = "El acreedor es un campo obligatorio")]
    public string Acreedor { get; set; }

    public string Periodicidad { get; set; } = Models.Periodicidad.Semanal.GetType().ToString();

    public bool Activa { get; set; } = true;
    
    public DateTime UltimaEjecuccion { get; set; } = DateTime.UtcNow;
}