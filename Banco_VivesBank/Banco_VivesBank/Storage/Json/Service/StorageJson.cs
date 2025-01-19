using Banco_VivesBank.Storage.Json.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace Banco_VivesBank.Storage.Json.Service;

public class StorageJson : IStorageJson
{
    private readonly ILogger<StorageJson> _logger;
    private readonly JsonSerializerSettings _jsonSettings;

    public StorageJson(ILogger<StorageJson> logger)
    {
        _logger = logger;
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() }
        };
    }
    
    public void ExportJson<T>(FileInfo file, List<T> data)
    {
        _logger.LogDebug($"Guardando datos de tipo {typeof(T).Name} en archivo JSON");

        try
        {
            // Usar Newtonsoft.Json.JsonConvert
            var json = JsonConvert.SerializeObject(data, _jsonSettings);
            File.WriteAllText(file.FullName, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar el archivo JSON de {typeof(T).Name}");
            throw new JsonStorageException("Ocurrió un error inesperado al guardar el archivo.", ex);
        }
    }
    
    public List<T> ImportJson<T>(FileInfo file)
    {
        _logger.LogDebug($"Cargando datos de tipo {typeof(T).Name} desde archivo JSON");

        try
        {
            var json = File.ReadAllText(file.FullName);
            return JsonConvert.DeserializeObject<List<T>>(json, _jsonSettings) ?? new List<T>();
        }
        catch (FileNotFoundException fnfEx)
        {
            _logger.LogError(fnfEx, "Archivo no encontrado");
            throw new JsonNotFoundException($"No se encontró el archivo para leer los datos de {typeof(T).Name}.", fnfEx);
        }
        catch (JsonReaderException jsonEx)
        {
            _logger.LogError(jsonEx, "Error al leer el archivo");
            throw new JsonReadException($"Error al procesar el archivo JSON de {typeof(T).Name}.", jsonEx);
        }
    }
}