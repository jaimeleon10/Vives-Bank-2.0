using System.Text.Json.Serialization;

namespace Banco_VivesBank.Producto.Tarjeta.Models;

public class TarjetaModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("Guid")]
    public string Guid { get; set; }
    
    [JsonPropertyName("numero")]
    public string Numero { get; set; }
    
    [JsonPropertyName("titular")]
    public string Titular { get; set; }
    
    [JsonPropertyName("fecha_vencimiento")]
    public string FechaVencimiento { get; set; }
    
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }
    
    [JsonPropertyName("pin")]
    public string Pin { get; set; }
    
    [JsonPropertyName("limiteDiario")]
    public double LimiteDiario { get; set; }
    
    [JsonPropertyName("limiteSemanal")]
    public double LimiteSemanal { get; set; }
    
    [JsonPropertyName("limiteMensual")]
    public double LimiteMensual { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; }
    
}