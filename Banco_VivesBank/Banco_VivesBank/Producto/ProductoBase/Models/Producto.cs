using System;
using Banco_VivesBank.Utils.Generators;
using Swashbuckle.AspNetCore.Annotations;

namespace Banco_VivesBank.Producto.ProductoBase.Models;

/// <summary>
/// Representa un producto dentro del sistema bancario VivesBank.
/// </summary>
public class Producto
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    [SwaggerSchema("Identificador único del producto", ReadOnly = true)]
    public long Id { get; set; } = 0;
    
    /// <summary>
    /// Identificador global único (GUID) del producto.
    /// </summary>
    [SwaggerSchema("Identificador global único (GUID) del producto", ReadOnly = true)]
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [SwaggerSchema("Nombre del producto", Nullable = false)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Descripción detallada del producto.
    /// </summary>
    [SwaggerSchema("Descripción detallada del producto", Nullable = true)]
    public string Descripcion  { get; set; } = "";
    
    /// <summary>
    /// Tipo de producto.
    /// </summary>
    [SwaggerSchema("Tipo de producto", Nullable = false)]
    public string TipoProducto { get; set; } = null!;

    /// <summary>
    /// Tasa Anual Equivalente (TAE) del producto.
    /// </summary>
    [SwaggerSchema("Tasa Anual Equivalente (TAE) del producto", Format = "double")]
    public double Tae  { get; set; } = 0.0;
    
    /// <summary>
    /// Fecha de creación del producto.
    /// </summary>
    [SwaggerSchema("Fecha de creación del producto", ReadOnly = true, Format = "date-time")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización del producto.
    /// </summary>
    [SwaggerSchema("Fecha de última actualización del producto", ReadOnly = true, Format = "date-time")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si el producto está eliminado (soft delete).
    /// </summary>
    [SwaggerSchema("Indica si el producto está eliminado (soft delete)")]
    public bool IsDeleted { get; set; } = false;
}