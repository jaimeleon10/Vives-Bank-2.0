using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Base.Models;

public class BaseModel
{
    
    [JsonPropertyName("id")]
    public long Id { get; set; } = 0;

    [JsonPropertyName("Guid ")] 
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("descripcion")] 
    public string Descripcion  { get; set; } = "";
    
    [JsonPropertyName("tipo_producto")]
    public string TipoProducto { get; set; } = null!;

    [JsonPropertyName("tae")] 
    public double Tae  { get; set; } = 0.0;
    
    [JsonPropertyName("createdAt")] 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
    
}