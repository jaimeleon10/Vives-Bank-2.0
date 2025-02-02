using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.ProductoBase.Dto;

/// <summary>
/// Representa los datos necesarios para crear un producto en el sistema.
/// </summary>
public class ProductoRequest
{
    /// <summary>
    /// Nombre del producto. Es un campo obligatorio con un máximo de 50 caracteres.
    /// </summary>
    /// <example>Producto A</example>
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción del producto. Es un campo obligatorio con un máximo de 1000 caracteres.
    /// </summary>
    /// <example>Descripción del producto A</example>
    [Required(ErrorMessage = "El campo descripcion es obligatorio")]
    [MaxLength(1000, ErrorMessage = "La descripcion no puede exceder los 1000 caracteres.")]
    public string Descripcion { get; set; }
    
    /// <summary>
    /// Tipo del producto. Es un campo obligatorio con un máximo de 1000 caracteres.
    /// </summary>
    /// <example>Tipo A</example>
    [Required(ErrorMessage = "El campo tipo es obligatorio")]
    [MaxLength(1000, ErrorMessage = "El tipo no puede exceder de los 1000 caracteres.")]
    public string TipoProducto { get; set; }
    
    /// <summary>
    /// Tasa Anual Equivalente (TAE) del producto. Es un campo obligatorio.
    /// </summary>
    /// <example>2.5</example>
    [Required(ErrorMessage = "El campo TAE es obligatorio")]
    public double Tae { get; set; }

    /// <summary>
    /// Indica si el producto ha sido marcado como eliminado. Por defecto es falso.
    /// </summary>
    /// <example>false</example>
    [System.ComponentModel.DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}