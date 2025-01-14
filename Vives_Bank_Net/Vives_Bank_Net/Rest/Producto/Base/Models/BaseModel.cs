using System.Text.Json.Serialization;
using Vives_Banks_Net.Utils.Generators;


namespace DefaultNamespace;

public class BaseModel
{
    
    [JsonPropertyName("id")]
    public long Id { get; set; } = 0;

    [JsonPropertyName("guid ")] 
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("descripcion")] 
    public string Descripcion  { get; set; } = "";

    [JsonPropertyName("tae")] 
    public double Tae  { get; set; } = 0.0;
    
    [JsonPropertyName("createdAt")] 
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
    
}