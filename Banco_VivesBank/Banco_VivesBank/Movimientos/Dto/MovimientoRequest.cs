using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Dto;

public class MovimientoRequest
{
    [Required(ErrorMessage = "El guid del cliente es obligatorio")]
    public string ClienteGuid { get; set; }
    
    public DomiciliacionRequest? Domiciliacion { get; set; } = null;

    public IngresoNominaRequest? IngresoNomina { get; set; } = null;

    public PagoConTarjetaRequest? PagoConTarjeta { get; set; } = null;

    public TransferenciaRequest? Transferencia { get; set; } = null;

}