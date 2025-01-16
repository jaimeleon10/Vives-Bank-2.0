using System.Numerics;
using System.Text.Json.Serialization;
using Vives_Bank_Net.Rest.Producto.Base.Models;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Models;


public class Cuenta
{
    [JsonPropertyName("id")]
    public long Id { get; set; } = 0;
    
    [JsonPropertyName("guid")]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [JsonPropertyName("iban")]
    public string Iban { get; set; } = IbanGenerator.GenerateIban();

    [JsonPropertyName("saldo")] 
    public BigInteger Saldo { get; set; } = 0;
    
    public long? TarjetaId { get; set; } = null;
    
    //[JsonPropertyName("tarjeta")] 
    //public Tarjeta? Tarjeta { get; set; } = null
    
    public long ClienteId { get; set; }

    [JsonPropertyName("cliente")] 
    public Cliente.Models.Cliente Cliente { get; set; }
    
    public long ProductoId { get; set; }
    
    [JsonPropertyName("producto")] 
    public BaseModel Producto { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }= DateTime.Now;
    
}