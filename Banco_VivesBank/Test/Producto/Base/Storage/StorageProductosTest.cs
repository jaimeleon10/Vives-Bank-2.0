using System.Text;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Base.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Storage;

public class StorageProductosTests
{
    private readonly Mock<ILogger<StorageProductos>> _loggerMock;
    private readonly StorageProductos _storage;
    private readonly string _tempPath;

    public StorageProductosTests()
    {
        _loggerMock = new Mock<ILogger<StorageProductos>>();
        _storage = new StorageProductos(_loggerMock.Object);
        _tempPath = Path.GetTempPath();
    }

    [Test]
    public void ImportProductos()
    {
        var csvContent = "nombre,descripcion,tipoProducto,tae\nCuenta,Cuenta Corriente,Cuenta,1.5";
        var filePath = Path.Combine(_tempPath, "test.csv");
        File.WriteAllText(filePath, csvContent, Encoding.UTF8);
        var fileInfo = new FileInfo(filePath);

        var result = _storage.ImportProductosFromCsv(fileInfo);

        Assert.That(result, Has.Exactly(1).Items);
        Assert.That(result[0].Nombre, Is.EqualTo("Cuenta"));
        Assert.That(result[0].Tae, Is.EqualTo(1.5));
    }

    [Test]
    public void ImportProductosArchivoInvalido()
    {
        var csvContent = "datos inválidos";
        var filePath = Path.Combine(_tempPath, "invalid.csv");
        File.WriteAllText(filePath, csvContent, Encoding.UTF8);
        var fileInfo = new FileInfo(filePath);

        var result = _storage.ImportProductosFromCsv(fileInfo);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ExportarProductos()
    {
        var productos = new List<Banco_VivesBank.Producto.Base.Models.Base>
        {
            new Banco_VivesBank.Producto.Base.Models.Base { Nombre = "Test", Descripcion = "Desc", TipoProducto = "Tipo", Tae = 2.5 }
        };
        var filePath = Path.Combine(_tempPath, "export.csv");
        var fileInfo = new FileInfo(filePath);

        _storage.ExportProductosFromCsv(fileInfo, productos);

        Assert.That(File.Exists(filePath) , Is.True);
        var lines = File.ReadAllLines(filePath);
        Assert.That(lines.Length, Is.EqualTo(2));
        Assert.That(lines[1], Does.Contain("Test"));
    }
}
