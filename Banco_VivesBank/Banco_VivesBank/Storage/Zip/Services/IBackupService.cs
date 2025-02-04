namespace Banco_VivesBank.Storage.Zip.Services;

/// <summary>
/// Interfaz que define los métodos para realizar operaciones de importación y exportación de datos utilizando archivos ZIP.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Importa datos desde un archivo ZIP a un directorio de destino.
    /// </summary>
    /// <param name="zipFilePath">Ruta completa del archivo ZIP que contiene los datos a importar.</param>
    /// <param name="destinationDirectory">Ruta completa del directorio donde se descomprimirán los archivos del ZIP.</param>
    /// <returns>Una tarea asincrónica que realiza la operación de importación.</returns>
    Task ImportFromZip(string zipFilePath, string destinationDirectory);
    
    /// <summary>
    /// Exporta archivos desde un directorio a un archivo ZIP.
    /// </summary>
    /// <param name="sourceDirectory">Ruta completa del directorio que contiene los archivos a exportar.</param>
    /// <param name="zipFilePath">Ruta completa del archivo ZIP donde se exportarán los archivos.</param>
    /// <returns>Una tarea asincrónica que realiza la operación de exportación.</returns>
    Task ExportToZip(string sourceDirectory, string zipFilePath);
}