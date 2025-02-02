namespace Banco_VivesBank.Websockets;

/// <summary>
/// Representa una notificación de WebSocket con detalles sobre la entidad afectada, el tipo de acción y la información relacionada.
/// </summary>
public class Notificacion
{
    /// <summary>
    /// Obtiene o establece el nombre de la entidad afectada por la notificación.
    /// </summary>
    public string Entity { get; set; }
    
    /// <summary>
    /// Obtiene o establece el tipo de acción que se realizó sobre la entidad.
    /// Este valor está basado en la enumeración <see cref="Tipo"/>.
    /// </summary>
    public Tipo Tipo { get; set; }
    
    /// <summary>
    /// Obtiene o establece los datos asociados con la notificación, generalmente en formato JSON.
    /// </summary>
    public string Data { get; set; }
    
    /// <summary>
    /// Obtiene o establece la fecha y hora en que se creó la notificación, representada como una cadena.
    /// </summary>
    public string CreatedAt { get; set; }
}