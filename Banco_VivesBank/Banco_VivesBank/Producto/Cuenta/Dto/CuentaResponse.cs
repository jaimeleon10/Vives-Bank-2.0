using System.Numerics;
using System.Text.Json.Serialization;

namespace Banco_VivesBank.Producto.Cuenta.Dto;

/// <summary>
/// Representa la respuesta con los datos de una cuenta.
/// </summary>
public class CuentaResponse
{
    /// <summary>
    /// Identificador único (GUID) de la cuenta.
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// IBAN de la cuenta.
    /// </summary>
    public required string Iban { get; set; }

    /// <summary>
    /// Saldo actual de la cuenta.
    /// </summary>
    public double Saldo { get; set; }

    /// <summary>
    /// GUID de la tarjeta asociada a la cuenta. Este campo puede ser nulo si no está asociada a una tarjeta.
    /// </summary>
    public string? TarjetaGuid { get; set; }

    /// <summary>
    /// GUID del cliente propietario de la cuenta.
    /// </summary>
    public string ClienteGuid { get; set; }

    /// <summary>
    /// GUID del producto asociado a la cuenta.
    /// </summary>
    public string ProductoGuid { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la cuenta.
    /// </summary>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización de la cuenta.
    /// </summary>
    public string UpdatedAt { get; set; }

    /// <summary>
    /// Indica si la cuenta ha sido eliminada.
    /// </summary>
    public bool IsDeleted { get; set; }
}
