using System.Numerics;
using System.Text.Json.Serialization;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Cuenta.Models;


/// <summary>
/// Representa una cuenta bancaria.
/// </summary>
public class Cuenta
{
    /// <summary>
    /// Identificador único de la cuenta (en base de datos).
    /// </summary>
    /// <example>1</example>
    public long Id { get; set; } = 0;

    /// <summary>
    /// Identificador único de la cuenta (GUID).
    /// </summary>
    /// <example>"a5c3f71e-85f2-4e6f-b6cf-72280a5de6a6"</example>
    public string Guid { get; set; } = GuidGenerator.GenerarId();

    /// <summary>
    /// IBAN de la cuenta.
    /// </summary>
    /// <example>"ES9121000418450200051332"</example>
    public string Iban { get; set; } = IbanGenerator.GenerateIban();

    /// <summary>
    /// Saldo actual de la cuenta.
    /// </summary>
    /// <example>1500.75</example>
    public double Saldo { get; set; } = 0;

    /// <summary>
    /// Tarjeta asociada a la cuenta, si existe. Este campo puede ser nulo.
    /// </summary>
    /// <example>null</example>
    public Tarjeta.Models.Tarjeta? Tarjeta { get; set; }

    /// <summary>
    /// Cliente propietario de la cuenta.
    /// </summary>
    /// <example>"d0721b9f-f7d1-4b09-87ff-e5063b7b4c8a"</example>
    public Cliente.Models.Cliente Cliente { get; set; }

    /// <summary>
    /// Producto asociado a la cuenta.
    /// </summary>
    /// <example>"ahorros"</example>
    public ProductoBase.Models.Producto Producto { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la cuenta.
    /// </summary>
    /// <example>"2025-02-02T14:30:00Z"</example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de la última actualización de la cuenta.
    /// </summary>
    /// <example>"2025-02-02T14:30:00Z"</example>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si la cuenta ha sido eliminada.
    /// </summary>
    /// <example>false</example>
    public bool IsDeleted { get; set; } = false;
}
