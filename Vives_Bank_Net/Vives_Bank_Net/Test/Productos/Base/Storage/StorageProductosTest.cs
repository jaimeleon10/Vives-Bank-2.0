using DefaultNamespace;
using Moq;
using NUnit.Framework;
using Vives_Bank_Net.Rest.Producto.Base.Storage;

namespace Vives_Bank_Net.Test.Productos.Base.Storage;

[TestFixture]
public class StorageProductosTest
{
    private readonly Mock<ILogger<StorageProductos>> _logger;
    private StorageProductos _storageProductos;

    public StorageProductosTest()
    {
        _logger = new Mock<ILogger<StorageProductos>>();
        _storageProductos = new StorageProductos(_logger.Object);
    }

    [Test]
    public void GuardarProductosValidosEnCsv()
    {
        var productos = new List<BaseModel>
        {
            new BaseModel
            {
                Id = 4,
                Guid = "4d56e890-12f3-45g6-7890-1b2c34d56789",
                Nombre = "CaixaBank Star",
                Descripcion = "Cuenta con ventajas para jóvenes",
                Tae = 1.0,
                CreatedAt = new DateTime(2025, 01, 15, 12, 0, 0),
                UpdatedAt = new DateTime(2025, 01, 15, 12, 0, 0),
                IsDeleted = false
            },
            new BaseModel
            {
                Id = 5,
                Guid = "5d67e890-12f3-45g6-7890-1b2c34d56789",
                Nombre = "CaixaBank Gold",
                Descripcion = "Cuenta con ventajas para adultos",
                Tae = 1.2,
                CreatedAt = new DateTime(2025, 01, 15, 12, 0, 0),
                UpdatedAt = new DateTime(2025, 01, 15, 12, 0, 0),
                IsDeleted = false
            }
        };

        var file = new FileInfo("productos.csv");
        _storageProductos.ExportProductosFromCsv(file, productos);

        Assert.That(file.Exists, Is.True);
        var lines = File.ReadAllLines(file.FullName);
        Assert.That(lines[0], Is.EqualTo("id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted"));
        Assert.That(lines[1], Is.EqualTo("4,4d56e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Star,Cuenta con ventajas para jóvenes,1,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False"));
        Assert.That(lines[2], Is.EqualTo("5,5d67e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Gold,Cuenta con ventajas para adultos,1.2,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False"));
    }
    
    [Test]
    public void ImportarProductosDesdeCsv()
    {
        var csvContent = new[]
        {
            "id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted",
            "4,4d56e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Star,Cuenta con ventajas para jóvenes,1,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False",
            "5,5d67e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Gold,Cuenta con ventajas para adultos,1.2,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False"
        };

        var file = new FileInfo("productos_import.csv");
        File.WriteAllLines(file.FullName, csvContent);

        var productos = _storageProductos.ImportProductosFromCsv(file);

        Assert.That(productos, Is.Not.Null);
        Assert.That(productos.Count, Is.EqualTo(2));
        Assert.That(productos[0].Id, Is.TypeOf<long>());
        Assert.That(productos[1].Id, Is.TypeOf<long>());
        Assert.That(productos[0].Nombre, Is.EqualTo("CaixaBank Star"));
        Assert.That(productos[0].Nombre, Is.TypeOf<String>());
        Assert.That(productos[0].Descripcion, Is.EqualTo("Cuenta con ventajas para jóvenes"));
        Assert.That(productos[0].Descripcion, Is.TypeOf<String>());
        Assert.That(productos[0].Tae, Is.EqualTo(1m));
        Assert.That(productos[0].Tae, Is.TypeOf<double>());
        Assert.That(productos[0].IsDeleted, Is.False);
        Assert.That(productos[0].IsDeleted, Is.TypeOf<bool>());
        Assert.That(productos[0].UpdatedAt, Is.TypeOf<DateTime>());
        Assert.That(productos[0].CreatedAt, Is.TypeOf<DateTime>());
        Assert.That(productos[1].Nombre, Is.EqualTo("CaixaBank Gold"));
        Assert.That(productos[1].Nombre, Is.TypeOf<String>());
        Assert.That(productos[1].Descripcion, Is.EqualTo("Cuenta con ventajas para adultos"));
        Assert.That(productos[1].Descripcion, Is.TypeOf<String>());
        Assert.That(productos[1].Tae, Is.EqualTo(1.2m));
        Assert.That(productos[1].Tae, Is.TypeOf<double>());
        Assert.That(productos[1].IsDeleted, Is.False);
        Assert.That(productos[1].IsDeleted, Is.TypeOf<bool>());
        Assert.That(productos[1].UpdatedAt, Is.TypeOf<DateTime>());
        Assert.That(productos[1].CreatedAt, Is.TypeOf<DateTime>());
    }
    [Test]
    public void ExportarProductos_ListaVacia()
    {
        var file = new FileInfo("productos_vacios.csv");
        _storageProductos.ExportProductosFromCsv(file, new List<BaseModel>());

        Assert.That(file.Exists, Is.True);

        var lines = File.ReadAllLines(file.FullName);
        Assert.That(lines.Length, Is.EqualTo(1));
        Assert.That(lines[0], Is.EqualTo("id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted"));
    }
    
    [Test]
    public void ImportarProductosArchivoVacio()
    {
        var csvContent = new[] { "id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted" };
        var file = new FileInfo("productos_vacios_import.csv");
        File.WriteAllLines(file.FullName, csvContent);

        var productos = _storageProductos.ImportProductosFromCsv(file);

        Assert.That(productos, Is.Not.Null);
        Assert.That(productos.Count, Is.EqualTo(0));
    }
}