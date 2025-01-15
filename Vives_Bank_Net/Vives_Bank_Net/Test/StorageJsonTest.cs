using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Vives_Banks_Net.Rest.User;

namespace Vives_Bank_Net.Test;

[TestFixture]
public class StorageJsonTest
{
    private readonly Mock<ILogger<StorageJson>> _logger;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly StorageJson _storageJson;

    public StorageJsonTest()
    {
        _logger = new Mock<ILogger<StorageJson>>();
        _jsonSettings = new JsonSerializerSettings();
        _storageJson = new StorageJson(_logger.Object);
    }

    [Test]
    public void ImportJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "testImport.json");
        var file = new FileInfo(path);
        var data1 = new User{Id = 1L, Guid = "guid", UserName = "username", PasswordHash = "password", Role = Role.USER, CreatedAt = DateTime.Parse("2025-01-14 10:30:00"), UpdatedAt = DateTime.Parse("2025-01-14 10:30:00"), IsDeleted = false};
        var data2 = new User{Id = 2L, Guid = "guid2", UserName = "username2", PasswordHash = "password2", Role = Role.USER, CreatedAt = DateTime.Parse("2025-01-13 10:30:00"), UpdatedAt = DateTime.Parse("2025-01-13 10:30:00"), IsDeleted = false};
        var data = new List<User>{data1, data2};
        
        _storageJson.ExportJson(file, data);
        _storageJson.ImportJson<List<User>>(file);
        
        var content = File.ReadAllText(path);
        var expectedJson = JsonConvert.DeserializeObject(data.ToString(), _jsonSettings);
        
        Assert.That(content.Replace("\r\n", "").Replace("\n", "").Replace(" ", ""), Is.EqualTo(expectedJson));
        Assert.That(content.Replace("\r\n", "").Replace("\n", "").Replace(" ", ""), Is.Not.Null);
    }
    
    [Test]
    public void ExportJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "testExport.json");
        var file = new FileInfo(path);
        var data1 = new User{Id = 1L, Guid = "guid", UserName = "username", PasswordHash = "password", Role = Role.USER, CreatedAt = DateTime.Parse("2025-01-14 10:30:00"), UpdatedAt = DateTime.Parse("2025-01-14 10:30:00"), IsDeleted = false};
        var data2 = new User{Id = 2L, Guid = "guid2", UserName = "username2", PasswordHash = "password2", Role = Role.USER, CreatedAt = DateTime.Parse("2025-01-13 10:30:00"), UpdatedAt = DateTime.Parse("2025-01-13 10:30:00"), IsDeleted = false};
        var data = new List<User>{data1, data2};
        
        _storageJson.ExportJson(file, data);
        
        Assert.That(File.Exists(path), Is.True);
        
        var content = File.ReadAllText(path);
        var expectedJson = JsonConvert.SerializeObject(data, _jsonSettings);
        expectedJson.Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
        
        Assert.That(content.Replace("\r\n", "").Replace("\n", "").Replace(" ", ""), Is.EqualTo(expectedJson));
        
        File.Delete(path);
    }
}