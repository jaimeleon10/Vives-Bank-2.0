namespace Banco_VivesBank.Producto.ProductoBase.Storage;

/// <summary>
/// Interfaz para la gestión del almacenamiento de productos.
/// </summary>
public interface IStorageProductos
{
    /// <summary>
    /// Importa productos desde un archivo CSV.
    /// </summary>
    /// <param name="file">Objeto FileInfo que representa el archivo CSV.</param>
    /// <returns>Lista de productos importados.</returns>
    List<ProductoBase.Models.Producto> ImportProductosFromCsv(FileInfo file);

    /// <summary>
    /// Exporta una lista de productos a un archivo CSV.
    /// </summary>
    /// <param name="file">Objeto FileInfo que representa el archivo CSV de destino.</param>
    /// <param name="data">Lista de productos a exportar.</param>
    void ExportProductosFromCsv(FileInfo file, List<ProductoBase.Models.Producto> data);
}