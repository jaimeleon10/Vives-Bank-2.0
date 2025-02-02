namespace Banco_VivesBank.Producto.Tarjeta.Dto;

/// <summary>
/// Representa la respuesta de una tarjeta en el sistema, proporcionando información relevante sin exponer detalles sensibles.
/// </summary>
public class TarjetaResponse
{
    /// <summary>
    /// Identificador único global (GUID) de la tarjeta.
    /// </summary>
    public string Guid { get; set; }

    /// <summary>
    /// Número de la tarjeta de crédito/débito. Puede estar enmascarado por seguridad.
    /// </summary>
    public string Numero { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la tarjeta en formato MM/YY.
    /// </summary>
    public string FechaVencimiento { get; set; }

    /// <summary>
    /// Código de seguridad de la tarjeta (CVV). Dependiendo del contexto, este valor podría no ser retornado por seguridad.
    /// </summary>
    public string Cvv { get; set; }

    /// <summary>
    /// PIN de seguridad de la tarjeta. Generalmente, no debería ser expuesto en las respuestas.
    /// </summary>
    public string Pin { get; set; }

    /// <summary>
    /// Límite de gasto diario permitido en la tarjeta.
    /// </summary>
    public double LimiteDiario { get; set; }

    /// <summary>
    /// Límite de gasto semanal permitido en la tarjeta.
    /// </summary>
    public double LimiteSemanal { get; set; }

    /// <summary>
    /// Límite de gasto mensual permitido en la tarjeta.
    /// </summary>
    public double LimiteMensual { get; set; }

    /// <summary>
    /// Fecha y hora en la que se creó el registro de la tarjeta, en formato de cadena.
    /// </summary>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización del registro de la tarjeta, en formato de cadena.
    /// </summary>
    public string UpdatedAt { get; set; }

    /// <summary>
    /// Indica si la tarjeta ha sido eliminada lógicamente del sistema.
    /// </summary>
    public bool IsDeleted { get; set; }
}
