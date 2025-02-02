using System.Globalization;
using System.Text;

namespace Banco_VivesBank.Producto.ProductoBase.Storage;
/// <summary>
/// Clase encargada del almacenamiento y procesamiento de productos mediante archivos CSV.
/// </summary>
public class StorageProductos : IStorageProductos
{
    private readonly ILogger<StorageProductos> _logger;

    /// <summary>
    /// Constructor de la clase StorageProductos.
    /// </summary>
    /// <param name="logger">Logger para el registro de eventos.</param>
    public StorageProductos(ILogger<StorageProductos> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Importa productos desde un archivo CSV.
    /// </summary>
    /// <param name="file">Objeto FileInfo que representa el archivo CSV.</param>
    /// <returns>Lista de productos importados.</returns>
    public List<ProductoBase.Models.Producto> ImportProductosFromCsv(FileInfo file)
    {
        _logger.LogDebug($"Importando productos desde CSV de {nameof(ProductoBase.Models.Producto)}");
        var productos = new List<ProductoBase.Models.Producto>();

        try
        {
            var lines = File.ReadAllLines(file.FullName, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                var data = line.Split(',').Select(field => field.Trim('"').Trim()).ToArray();

                if (data.Length != 4)
                {
                    _logger.LogWarning($"Línea omitida debido a número incorrecto de columnas: {line}");
                    continue;
                }

                var producto = new ProductoBase.Models.Producto
                {
                    Nombre = data[0],
                    Descripcion = data[1],
                    TipoProducto = data[2],
                    Tae = double.Parse(data[3], CultureInfo.InvariantCulture)
                };
                productos.Add(producto);
            }
            return productos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al procesar el fichero CSV: {ex.Message}");
            return new List<ProductoBase.Models.Producto>();
        }
    }
    
    /// <summary>
    /// Exporta una lista de productos a un archivo CSV.
    /// </summary>
    /// <param name="file">Objeto FileInfo que representa el archivo CSV de destino.</param>
    /// <param name="data">Lista de productos a exportar.</param>
    public void ExportProductosFromCsv(FileInfo file, List<ProductoBase.Models.Producto> data)
    {
        _logger.LogDebug($"Exportando productos a CSV de {nameof(ProductoBase.Models.Producto)}");
        try
        {
            var lines = new List<string> { "nombre,descripcion,tipoProducto,tae" };
            foreach (var producto in data)
            {
                lines.Add($"{producto.Nombre},{producto.Descripcion},{producto.TipoProducto},{producto.Tae.ToString(CultureInfo.InvariantCulture)}");
            }
            File.WriteAllLines(file.FullName, lines, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al exportar el fichero CSV: {ex.Message}");
        }
    }
}