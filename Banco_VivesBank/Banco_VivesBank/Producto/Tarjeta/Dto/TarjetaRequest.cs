using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.Tarjeta.Dto;

public class TarjetaRequest
{
    
    /// <summary>
    /// Identificador único global (GUID) de la cuenta asociada a la tarjeta.
    /// </summary>
    [Required(ErrorMessage = "El campo de cuenta guid es obligatorio")]
    public string CuentaGuid { get; set; }
    
    /// <summary>
    /// PIN de seguridad de la tarjeta. Generalmente, no debería ser expuesto en las respuestas.
    /// </summary>
    [Required(ErrorMessage = "Es necesario definir un Pin")]
    [Length(4, 4, ErrorMessage = "El pin debe tener una longitud de 4 caracteres")]
    public string Pin { get; set; }

    /// <summary>
    /// Límite de gasto diario permitido en la tarjeta.
    /// </summary>
    [Required(ErrorMessage = "Debes establecer un limite diario superior a 0")]
    public double LimiteDiario { get; set; }
    
    /// <summary>
    /// Límite de gasto semanal permitido en la tarjeta.
    /// </summary>
    [Required(ErrorMessage = "Debes establecer un limite semanal superior a 0 y al limite diario")]
    public double LimiteSemanal { get; set; }

    /// <summary>
    /// Límite de gasto mensual permitido en la tarjeta.
    /// </summary>
    [Required(ErrorMessage = "Debes establecer un limite mensual superior a 0 y al limite semanal")]
    public double LimiteMensual { get; set; }
    
}