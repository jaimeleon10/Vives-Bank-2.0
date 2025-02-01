namespace Banco_VivesBank.Websockets;

public class Notificacion
{
    public string Entity { get; set; }
    
    public Tipo Tipo { get; set; }
    
    public string Data { get; set; }
    
    public string CreatedAt { get; set; }
}