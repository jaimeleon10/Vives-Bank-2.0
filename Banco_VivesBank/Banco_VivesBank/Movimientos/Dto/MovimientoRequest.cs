using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Dto;

public class MovimientoRequest
{
    [Required(ErrorMessage = "El guid del cliente es obligatorio")]
    public string ClienteGuid { get; set; }
    
    public Domiciliacion? Domiciliacion { get; set; } = null;

    public IngresoNomina? IngresoNomina { get; set; } = null;

    public PagoConTarjeta? PagoConTarjeta { get; set; } = null;

    public Transferencia? Transferencia { get; set; } = null;
}