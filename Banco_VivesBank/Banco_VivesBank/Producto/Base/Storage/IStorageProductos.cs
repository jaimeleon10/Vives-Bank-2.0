
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Storage;

public interface IStorageProductos
{
    List<BaseModel> ImportProductosFromCsv(FileInfo file);
    void ExportProductosFromCsv(FileInfo file, List<BaseModel> data);
}