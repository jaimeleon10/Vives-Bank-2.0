using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.Cuenta.Dto;

/// <summary>
/// Representa la solicitud para crear una cuenta.
/// </summary>
public class CuentaRequest
{
    /// <summary>
    /// Tipo de cuenta que se desea crear.
    /// </summary>
    /// <remarks>Este campo es obligatorio y debe especificar el tipo de cuenta, por ejemplo: 'Ahorros', 'Corriente', etc.</remarks>
    [Required(ErrorMessage = "El campo tipoCuenta es obligatorio")]
    public string TipoCuenta { get; set; }
}