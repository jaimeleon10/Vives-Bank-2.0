namespace Vives_Bank_Net.Rest.Producto.Base.Dto;

public class BaseResponseDto
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public double Tae { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
    