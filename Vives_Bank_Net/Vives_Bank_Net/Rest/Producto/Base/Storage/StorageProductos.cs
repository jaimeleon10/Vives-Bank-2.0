namespace Vives_Bank_Net.Rest.Producto.Base.Storage;

using System.Globalization;
using System.Text;
using DefaultNamespace;

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
                    Id = long.Parse(data[0].Trim()),
                    Guid = data[1].Trim(),
                    Nombre = data[2].Trim(),
                    Descripcion = data[3].Trim(),
                    Tae = double.Parse(data[4].Trim(), CultureInfo.InvariantCulture),
                    CreatedAt = DateTime.Parse(data[5].Trim(), CultureInfo.InvariantCulture),
                    UpdatedAt = DateTime.Parse(data[6].Trim(), CultureInfo.InvariantCulture),
                    IsDeleted = bool.Parse(data[7].Trim())
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
            var lines = new List<string> { "id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted" };

            for (int i = 0; i < data.Count; i++)
            {
                var producto = data[i];
                lines.Add($"{producto.Id},{producto.Guid},{producto.Nombre},{producto.Descripcion},{producto.Tae.ToString(CultureInfo.InvariantCulture)},{producto.CreatedAt.ToString("o")},{producto.UpdatedAt.ToString("o")},{producto.IsDeleted}");
            }

            File.WriteAllLines(file.FullName, lines, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al exportar el fichero CSV: {ex.Message}");
        }
    }
}