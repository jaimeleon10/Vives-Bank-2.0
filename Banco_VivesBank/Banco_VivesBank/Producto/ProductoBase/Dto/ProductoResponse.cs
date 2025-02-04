namespace Banco_VivesBank.Producto.ProductoBase.Dto;

/// <summary>
/// Representa la respuesta que se devuelve al cliente al solicitar los detalles de un producto.
/// </summary>
public class ProductoResponse
{
    /// <summary>
    /// Identificador único del producto (GUID).
    /// </summary>
    /// <example>"123e4567-e89b-12d3-a456-426614174000"</example>
    public string Guid { get; set; }

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    /// <example>Producto A</example>
    public string Nombre { get; set; }

    /// <summary>
    /// Descripción detallada del producto.
    /// </summary>
    /// <example>Descripción completa del producto A.</example>
    public string Descripcion { get; set; }

    /// <summary>
    /// Tipo del producto.
    /// </summary>
    /// <example>Tipo A</example>
    public string TipoProducto { get; set; }

    /// <summary>
    /// Tasa Anual Equivalente (TAE) del producto.
    /// </summary>
    /// <example>2.5</example>
    public double Tae { get; set; }

    /// <summary>
    /// Fecha y hora de creación del producto en formato ISO 8601.
    /// </summary>
    /// <example>"2025-01-01T12:00:00"</example>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización del producto en formato ISO 8601.
    /// </summary>
    /// <example>"2025-01-02T14:30:00"</example>
    public string UpdatedAt { get; set; }

    /// <summary>
    /// Indica si el producto ha sido marcado como eliminado.
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; }
}