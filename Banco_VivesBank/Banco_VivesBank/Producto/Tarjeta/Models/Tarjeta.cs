using Banco_VivesBank.Utils.Generators;
using Swashbuckle.AspNetCore.Annotations;

namespace Banco_VivesBank.Producto.Tarjeta.Models;

    /// <summary>
    /// Representa una tarjeta de crédito o débito con información relevante.
    /// </summary>
    public class Tarjeta
    {
        /// <summary>
        /// El identificador único de la tarjeta.
        /// </summary>
        /// <example>1</example>
        [SwaggerSchema("El identificador único de la tarjeta.")]
        public long Id { get; set; }
        
        /// <summary>
        /// El identificador GUID de la tarjeta, generado automáticamente.
        /// </summary>
        /// <example>"5zXnkqio9jP"</example>
        [SwaggerSchema("El identificador GUID de la tarjeta.")]
        public string Guid { get; set; } = GuidGenerator.GenerarId();
        
        /// <summary>
        /// El número de la tarjeta de crédito o débito.
        /// </summary>
        /// <example>"1234 5678 9012 3456"</example>
        [SwaggerSchema("El número de la tarjeta de crédito o débito.")]
        public string Numero { get; set; }
        
        /// <summary>
        /// La fecha de vencimiento de la tarjeta en formato MM/AA.
        /// </summary>
        /// <example>"12/25"</example>
        [SwaggerSchema("La fecha de vencimiento de la tarjeta en formato MM/AA.")]
        public string FechaVencimiento { get; set; }
        
        /// <summary>
        /// El código de seguridad (CVV) de la tarjeta.
        /// </summary>
        /// <example>"123"</example>
        [SwaggerSchema("El código de seguridad (CVV) de la tarjeta.")]
        public string Cvv { get; set; }
        
        /// <summary>
        /// El PIN de la tarjeta, utilizado para transacciones.
        /// </summary>
        /// <example>"1234"</example>
        [SwaggerSchema("El PIN de la tarjeta, utilizado para transacciones.")]
        public string Pin { get; set; }
        
        /// <summary>
        /// El límite diario permitido para la tarjeta.
        /// </summary>
        /// <example>1000.00</example>
        [SwaggerSchema("El límite diario permitido para la tarjeta.")]
        public double LimiteDiario { get; set; }
        
        /// <summary>
        /// El límite semanal permitido para la tarjeta.
        /// </summary>
        /// <example>5000.00</example>
        [SwaggerSchema("El límite semanal permitido para la tarjeta.")]
        public double LimiteSemanal { get; set; }
        
        /// <summary>
        /// El límite mensual permitido para la tarjeta.
        /// </summary>
        /// <example>20000.00</example>
        [SwaggerSchema("El límite mensual permitido para la tarjeta.")]
        public double LimiteMensual { get; set; }
        
        /// <summary>
        /// La fecha y hora de creación de la tarjeta.
        /// </summary>
        /// <example>"2025-02-02T00:00:00Z"</example>
        [SwaggerSchema("La fecha y hora de creación de la tarjeta.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// La fecha y hora de la última actualización de la tarjeta.
        /// </summary>
        /// <example>"2025-02-02T00:00:00Z"</example>
        [SwaggerSchema("La fecha y hora de la última actualización de la tarjeta.")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Indica si la tarjeta ha sido eliminada.
        /// </summary>
        /// <example>false</example>
        [SwaggerSchema("Indica si la tarjeta ha sido eliminada.")]
        public bool IsDeleted { get; set; }
    }
