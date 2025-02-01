using System.Globalization;
using System.Text;
using Banco_VivesBank.Producto.Base.Storage;

namespace Banco_VivesBank.Producto.ProductoBase.Storage;
public class StorageProductos : IStorageProductos
{
    private readonly ILogger<StorageProductos> _logger;

    public StorageProductos(ILogger<StorageProductos> logger)
    {
        _logger = logger;
    }

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