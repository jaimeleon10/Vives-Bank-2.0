
using Vives_Bank_Net.Rest.Producto.Base.Models;

namespace Vives_Bank_Net.Rest.Producto.Base.Storage;

public interface IStorageProductos
{
    List<BaseModel> ImportProductosFromCsv(FileInfo file);
    void ExportProductosFromCsv(FileInfo file, List<BaseModel> data);
}