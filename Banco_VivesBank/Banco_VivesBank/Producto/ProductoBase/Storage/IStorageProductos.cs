
namespace Banco_VivesBank.Producto.Base.Storage;

public interface IStorageProductos
{
    List<ProductoBase.Models.Producto> ImportProductosFromCsv(FileInfo file);
    void ExportProductosFromCsv(FileInfo file, List<ProductoBase.Models.Producto> data);
}