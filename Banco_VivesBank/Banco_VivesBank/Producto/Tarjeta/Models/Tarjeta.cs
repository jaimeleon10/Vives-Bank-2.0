using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Tarjeta.Models;

public class Tarjeta
{
    public long Id { get; set; }
    
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    public string Numero { get; set; }
    
    public string FechaVencimiento { get; set; }
    
    public string Cvv { get; set; }
    
    public string Pin { get; set; }
    
    public double LimiteDiario { get; set; }
    
    public double LimiteSemanal { get; set; }
    
    public double LimiteMensual { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; }
}