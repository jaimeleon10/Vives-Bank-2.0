
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Storage;

public interface IStorageProductos
{
    List<Models.Base> ImportProductosFromCsv(FileInfo file);
    void ExportProductosFromCsv(FileInfo file, List<Models.Base> data);
}