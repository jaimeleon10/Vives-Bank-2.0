using System.Globalization;
using System.Text;
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Storage;

public class StorageProductos : IStorageProductos
{
    private readonly ILogger<StorageProductos> _logger;

    public StorageProductos(ILogger<StorageProductos> logger)
    {
        _logger = logger;
    }

    public List<BaseModel> ImportProductosFromCsv(FileInfo file)
    {
        _logger.LogDebug($"Importando productos desde CSV de {nameof(BaseModel)}");
        var productos = new List<BaseModel>();

        try
        {
            var lines = File.ReadAllLines(file.FullName, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                var data = line.Split(',');
                var producto = new BaseModel
                {
                    Nombre = data[1].Trim(),
                    Descripcion = data[2].Trim(),
                    TipoProducto = data[3].Trim(),
                    Tae = double.Parse(data[4].Trim(), CultureInfo.InvariantCulture)
                };
                productos.Add(producto);
            }
            return productos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al procesar el fichero CSV: {ex.Message}");
            return new List<BaseModel>();
        }
    }

    public void ExportProductosFromCsv(FileInfo file, List<BaseModel> data)
    {
        _logger.LogDebug($"Exportando productos a CSV de {nameof(BaseModel)}");
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