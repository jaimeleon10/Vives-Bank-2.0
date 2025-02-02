namespace Banco_VivesBank.Storage.Images.Service;

/// <summary>
/// Interfaz para el servicio de almacenamiento de archivos.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo en el almacenamiento.
    /// </summary>
    /// <param name="file">Archivo a almacenar.</param>
    /// <returns>Nombre del archivo guardado.</returns>
    Task<string> SaveFileAsync(IFormFile file);

    /// <summary>
    /// Obtiene un archivo del almacenamiento.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a recuperar.</param>
    /// <returns>Stream del archivo solicitado.</returns>
    Task<FileStream> GetFileAsync(string fileName);

    /// <summary>
    /// Elimina un archivo del almacenamiento.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a eliminar.</param>
    /// <returns>True si el archivo fue eliminado correctamente, false en caso contrario.</returns>
    Task<bool> DeleteFileAsync(string fileName);
}