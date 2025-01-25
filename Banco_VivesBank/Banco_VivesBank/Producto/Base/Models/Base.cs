using System.Text.Json.Serialization;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Base.Models;

public class Base
{
    public long Id { get; set; } = 0;
    
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    public string Nombre { get; set; } = null!;

    public string Descripcion  { get; set; } = "";
    
    public string TipoProducto { get; set; } = null!;

    public double Tae  { get; set; } = 0.0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
}