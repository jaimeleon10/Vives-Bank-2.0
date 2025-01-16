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

        //creamos el FileInfo
        var file = new FileInfo("productos.csv");

        //lanzamos la exportación
        _storageProductos.ExportProductosFromCsv(file, productos);

        //comprobamos que se ha creado el fichero
        Assert.That(file.Exists, Is.True);
        
        //comprobamos que el contenido del fichero es correcto
        var lines = File.ReadAllLines(file.FullName);
        Assert.That(lines[0], Is.EqualTo("id,guid,nombre,descripcion,tae,createdAt,updatedAt,isDeleted"));
        Assert.That(lines[1], Is.EqualTo("4,4d56e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Star,Cuenta con ventajas para jóvenes,1,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False"));
        Assert.That(lines[2],
            Is.EqualTo(
                "5,5d67e890-12f3-45g6-7890-1b2c34d56789,CaixaBank Gold,Cuenta con ventajas para adultos,1.2,2025-01-15T12:00:00.0000000,2025-01-15T12:00:00.0000000,False"));
    }

}