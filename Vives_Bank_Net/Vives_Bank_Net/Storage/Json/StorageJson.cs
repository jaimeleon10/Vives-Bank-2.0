using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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
            //Lanzamos excepcion
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al leer el archivo JSON de {typeof(T).Name}");
            return new List<T>();
        }
    }
}