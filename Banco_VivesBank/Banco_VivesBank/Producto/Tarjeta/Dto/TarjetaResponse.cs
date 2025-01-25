namespace Banco_VivesBank.Producto.Tarjeta.Dto;

public class TarjetaResponse
{
    public long Id { get; set; }
    public string Guid { get; set; }
    public string Numero { get; set; }
    public string FechaVencimiento { get; set; }
    public string Cvv { get; set; }
    public string Pin { get; set; }
    public double LimiteDiario { get; set; }
    public double LimiteSemanal { get; set; }
    public double LimiteMensual { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}