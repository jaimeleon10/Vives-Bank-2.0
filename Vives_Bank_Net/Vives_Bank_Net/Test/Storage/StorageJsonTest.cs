using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Vives_Bank_Net.Storage.Json.Exceptions;
using Vives_Bank_Net.Storage.Json.Service;
using Vives_Bank_Net.Rest.User;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Test.Storage;

[TestFixture]
public class StorageJsonTest
{
    private readonly Mock<ILogger<StorageJson>> _logger;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly StorageJson _storageJson;

    public StorageJsonTest()
    {
        _logger = new Mock<ILogger<StorageJson>>();
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() },
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };
        _storageJson = new StorageJson(_logger.Object);
    }

    [Test]
    public void ImportJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "testImport.json");
        var file = new FileInfo(path);

        var data1 = new User
        {
            Id = 1L,
            Guid = "guid",
            UserName = "username",
            Password = "password",
            Role = Role.USER,
            CreatedAt = DateTime.Parse("2025-01-14 10:30:00"),
            UpdatedAt = DateTime.Parse("2025-01-14 10:30:00"),
            IsDeleted = false
        };

        var data2 = new User
        {
            Id = 2L,
            Guid = "guid2",
            UserName = "username2",
            Password = "password2",
            Role = Role.USER,
            CreatedAt = DateTime.Parse("2025-01-13 10:30:00"),
            UpdatedAt = DateTime.Parse("2025-01-13 10:30:00"),
            IsDeleted = false
        };

        var data = new List<User> { data1, data2 };

        _storageJson.ExportJson(file, data);

        Assert.That(File.Exists(file.FullName), Is.True);
        
        var content = File.ReadAllText(file.FullName);
        Assert.That(!string.IsNullOrEmpty(content), Is.True);
        
        var contentDeserializado = JsonConvert.DeserializeObject<List<User>>(content, _jsonSettings);
        
        Assert.That(contentDeserializado.Count, Is.EqualTo(data.Count));
        for (int i = 0; i < data.Count; i++)
        {
            Assert.That(contentDeserializado[i].Id, Is.EqualTo(data[i].Id));
            Assert.That(contentDeserializado[i].Guid, Is.EqualTo(data[i].Guid));
            Assert.That(contentDeserializado[i].UserName, Is.EqualTo(data[i].UserName));
            Assert.That(contentDeserializado[i].Password, Is.EqualTo(data[i].Password));
            Assert.That(contentDeserializado[i].Role, Is.EqualTo(data[i].Role));
            Assert.That(contentDeserializado[i].CreatedAt, Is.EqualTo(data[i].CreatedAt));
            Assert.That(contentDeserializado[i].UpdatedAt, Is.EqualTo(data[i].UpdatedAt));
            Assert.That(contentDeserializado[i].IsDeleted, Is.EqualTo(data[i].IsDeleted));
        }
    }
    
    [Test]
    public void ImportJsonInvalido()
    {
        var path = Path.Combine(Path.GetTempPath(), "testImport.json");
        var file = new FileInfo(path);

        File.WriteAllText(path, "{ json invalido }");

        var exception = Assert.Throws<JsonReadException>(() => _storageJson.ImportJson<User>(file));
        Assert.That(exception.Message, Is.EqualTo("Error al procesar el archivo JSON de User."));
    }

    [Test]
    public void ImportJsonFileNotFound()
    {
        var path = Path.Combine(Path.GetTempPath(), "inexistente.json");
        var file = new FileInfo(path);

        var exception = Assert.Throws<JsonNotFoundException>(() => _storageJson.ImportJson<User>(file));

        Assert.That(exception.Message, Is.EqualTo("No se encontró el archivo para leer los datos de User."));
    }
    
    [Test]
    public void ExportJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "testExport.json");
        var file = new FileInfo(path);
        
        var data1 = new User
        {
            Id = 1L,
            Guid = "guid",
            UserName = "username",
            Password = "password",
            Role = Role.USER,
            CreatedAt = DateTime.Parse("2025-01-14 10:30:00"),
            UpdatedAt = DateTime.Parse("2025-01-14 10:30:00"),
            IsDeleted = false
        };

        var data2 = new User
        {
            Id = 2L,
            Guid = "guid2",
            UserName = "username2",
            Password = "password2",
            Role = Role.USER,
            CreatedAt = DateTime.Parse("2025-01-13 10:30:00"),
            UpdatedAt = DateTime.Parse("2025-01-13 10:30:00"),
            IsDeleted = false
        };
        
        var data = new List<User>{data1, data2};
        
        _storageJson.ExportJson(file, data);
        
        Assert.That(File.Exists(path), Is.True);
        
        var content = File.ReadAllText(path);
        var expectedJson = JsonConvert.SerializeObject(data, _jsonSettings);
        
        Assert.That(content, Is.EqualTo(expectedJson));
        
        File.Delete(path);
    }
    
    [Test]
    public void ExportJsonErrorSerializacion()
    {
        var fileInfo = new FileInfo("path/to/file.json");
        var data = new List<object> { new { Property = "value" } };
        
        var exception = Assert.Throws<JsonStorageException>(() => _storageJson.ExportJson(fileInfo, data));
        Assert.That(exception.Message, Is.EqualTo("Ocurrió un error inesperado al guardar el archivo."));
    }
}