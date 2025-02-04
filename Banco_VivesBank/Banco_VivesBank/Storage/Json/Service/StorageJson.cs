using Banco_VivesBank.Storage.Json.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace Banco_VivesBank.Storage.Json.Service;

/// <summary>
/// Clase que implementa la interfaz <see cref="IStorageJson"/> para la gestión de operaciones de lectura y escritura de datos en formato JSON.
/// Proporciona métodos para exportar y importar datos a y desde archivos JSON, con manejo de excepciones personalizadas.
/// </summary>
public class StorageJson : IStorageJson
{
    private readonly ILogger<StorageJson> _logger;
    private readonly JsonSerializerSettings _jsonSettings;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="StorageJson"/> con el registro de logs.
    /// Configura las opciones del serializador JSON, incluyendo el formato de indentación y el uso de resolutores de nombres de propiedad.
    /// </summary>
    /// <param name="logger">Instancia del logger para registrar los eventos.</param>
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
    
    /// <summary>
    /// Exporta una lista de datos genéricos a un archivo JSON.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos a exportar.</typeparam>
    /// <param name="file">Archivo donde se guardarán los datos en formato JSON.</param>
    /// <param name="data">Lista de datos a exportar.</param>
    /// <exception cref="JsonStorageException">Lanzada si ocurre un error inesperado durante la escritura en el archivo.</exception>
    public void ExportJson<T>(FileInfo file, List<T> data)
    {
        _logger.LogDebug($"Guardando datos de tipo {typeof(T).Name} en archivo JSON");

        try
        {
            var json = JsonConvert.SerializeObject(data, _jsonSettings);
            File.WriteAllText(file.FullName, json);
            _logger.LogInformation($"Archivo JSON guardado correctamente en {file.FullName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar el archivo JSON de {typeof(T).Name}");
            throw new JsonStorageException("Ocurrió un error inesperado al guardar el archivo.", ex);
        }
    }
    
    /// <summary>
    /// Importa una lista de datos genéricos desde un archivo JSON.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos a importar.</typeparam>
    /// <param name="file">Archivo JSON desde el cual se cargarán los datos.</param>
    /// <returns>Lista de datos importados del archivo JSON.</returns>
    /// <exception cref="JsonNotFoundException">Lanzada si el archivo no se encuentra.</exception>
    /// <exception cref="JsonReadException">Lanzada si ocurre un error al leer o procesar el archivo JSON.</exception>
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