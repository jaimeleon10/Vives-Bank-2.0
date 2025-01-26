namespace Banco_VivesBank.Producto.ProductoBase.Dto;

public class ProductoResponse
{
    public string Guid { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string TipoProducto { get; set; }
    public double Tae { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
    