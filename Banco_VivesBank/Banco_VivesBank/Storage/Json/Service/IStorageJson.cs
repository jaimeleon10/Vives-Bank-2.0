namespace Banco_VivesBank.Storage.Json.Service;

public interface IStorageJson
{
    void ExportJson<T>(FileInfo file, List<T> data);
    
    List<T> ImportJson<T>(FileInfo file);
}