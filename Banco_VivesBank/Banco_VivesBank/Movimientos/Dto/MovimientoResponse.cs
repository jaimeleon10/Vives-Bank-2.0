using System.Text.Json.Serialization;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Dto;

public class MovimientoResponse
{
    public required string Guid { get; set; }
    
    public required string ClienteGuid { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DomiciliacionResponse? Domiciliacion { get; set; } = null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IngresoNominaResponse? IngresoNomina { get; set; } = null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PagoConTarjetaResponse? PagoConTarjeta { get; set; } = null;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TransferenciaResponse? Transferencia { get; set; } = null;
    
    public required string CreatedAt { get; set; }
}