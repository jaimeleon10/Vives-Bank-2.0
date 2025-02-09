namespace Banco_VivesBank.Storage.Json.Service;

/// <summary>
/// Interfaz que define las operaciones básicas de almacenamiento y recuperación de datos en formato JSON.
/// Esta interfaz es implementada por clases que gestionan la exportación e importación de datos a y desde archivos JSON.
/// </summary>
public interface IStorageJson
{
    /// <summary>
    /// Exporta una lista de objetos genéricos a un archivo JSON.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos a exportar.</typeparam>
    /// <param name="file">Archivo donde se guardarán los datos en formato JSON.</param>
    /// <param name="data">Lista de datos a exportar.</param>
    Task ExportJson<T>(FileInfo file, List<T> data);
    
    /// <summary>
    /// Importa una lista de objetos genéricos desde un archivo JSON.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos a importar.</typeparam>
    /// <param name="file">Archivo JSON desde el cual se cargarán los datos.</param>
    /// <returns>Un observable con los datos que se importan del archivo JSON.</returns>
    IObservable<T> ImportJson<T>(FileInfo file);
}