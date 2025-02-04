using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Producto.ProductoBase.Mappers;

namespace Test.Producto.Base.Mappers;

public class ProductoMapperTests
{
    [Test]
    public void ToModelFromRequest()
    {
        var request = new ProductoRequest
        {
            Nombre = "Test Producto",
            Descripcion = "Test Descripcion",
            Tae = 5.5,
            TipoProducto = "Test Tipo"
        };

        var result = ProductoMapper.ToModelFromRequest(request);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Nombre, Is.EqualTo(request.Nombre));
        Assert.That(result.Descripcion, Is.EqualTo(request.Descripcion));
        Assert.That(result.Tae, Is.EqualTo(request.Tae));
        Assert.That(result.TipoProducto, Is.EqualTo(request.TipoProducto));
        Assert.That(result.IsDeleted, Is.False);
        Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(result.UpdatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void ToModelFromEntity()
    {
        var entity = new ProductoEntity
        {
            Id = 1,
            Nombre = "Test Producto",
            Descripcion = "Test Descripcion",
            Tae = 5.5,
            TipoProducto = "Test Tipo",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ProductoMapper.ToModelFromEntity(entity);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entity.Id));
        Assert.That(result.Nombre, Is.EqualTo(entity.Nombre));
        Assert.That(result.Descripcion, Is.EqualTo(entity.Descripcion));
        Assert.That(result.Tae, Is.EqualTo(entity.Tae));
        Assert.That(result.TipoProducto, Is.EqualTo(entity.TipoProducto));
        Assert.That(result.CreatedAt, Is.EqualTo(entity.CreatedAt));
        Assert.That(result.UpdatedAt, Is.EqualTo(entity.UpdatedAt));
        Assert.That(result.IsDeleted, Is.EqualTo(entity.IsDeleted));
    }

    [Test]
    public void ToEntityFromModel()
    {
        var model = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Id = 1,
            Nombre = "Test Producto",
            Descripcion = "Test Descripcion",
            Tae = 5.5,
            TipoProducto = "Test Tipo",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ProductoMapper.ToEntityFromModel(model);

        Assert.That(result, Is.Not.Null);
        Assert.That(model.Id, Is.EqualTo(result.Id));
        Assert.That(model.Nombre, Is.EqualTo(result.Nombre));
        Assert.That(model.Descripcion, Is.EqualTo(result.Descripcion));
        Assert.That(model.Tae, Is.EqualTo(result.Tae));
        Assert.That(model.TipoProducto, Is.EqualTo(result.TipoProducto));
        Assert.That(model.CreatedAt, Is.EqualTo(result.CreatedAt));
        Assert.That(model.UpdatedAt, Is.EqualTo(result.UpdatedAt));
        Assert.That(model.IsDeleted, Is.EqualTo(result.IsDeleted));
    }

    [Test]
    public void ToResponseFromModel()
    {
        var model = new Banco_VivesBank.Producto.ProductoBase.Models.Producto
        {
            Id = 1,
            Nombre = "Test Producto",
            Descripcion = "Test Description",
            Tae = 5.5,
            TipoProducto = "Test Type",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ProductoMapper.ToResponseFromModel(model);

        Assert.That(result, Is.Not.Null);
        Assert.That(model.Nombre, Is.EqualTo(result.Nombre));
        Assert.That(model.Descripcion, Is.EqualTo(result.Descripcion));
        Assert.That(model.Tae, Is.EqualTo(result.Tae));
        Assert.That(model.TipoProducto, Is.EqualTo(result.TipoProducto));
        
        var formattedCreatedAt = model.CreatedAt.ToString();
        Assert.That(formattedCreatedAt, Is.EqualTo(result.CreatedAt));

        var formattedUpdatedAt = model.UpdatedAt.ToString();
        Assert.That(formattedUpdatedAt, Is.EqualTo(result.UpdatedAt));
        
        Assert.That(model.IsDeleted, Is.EqualTo(result.IsDeleted));
    }

    [Test]
    public void ToResponseFromEntity()
    {
        var entity = new ProductoEntity
        {
            Id = 1,
            Nombre = "Test Producto",
            Descripcion = "Test Descripcion",
            Tae = 5.5,
            TipoProducto = "Test Tipo",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ProductoMapper.ToResponseFromEntity(entity);

        Assert.That(result, Is.Not.Null);
        Assert.That(entity.Nombre, Is.EqualTo(result.Nombre));
        Assert.That(entity.Descripcion, Is.EqualTo(result.Descripcion));
        Assert.That(entity.Tae, Is.EqualTo(result.Tae));
        Assert.That(entity.TipoProducto, Is.EqualTo(result.TipoProducto));

        var formattedCreatedAt = entity.CreatedAt.ToString();
        Assert.That(formattedCreatedAt, Is.EqualTo(result.CreatedAt));

        var formattedUpdatedAt = entity.UpdatedAt.ToString();
        Assert.That(formattedUpdatedAt, Is.EqualTo(result.UpdatedAt));

        Assert.That(entity.IsDeleted, Is.EqualTo(result.IsDeleted));
    }

    [Test]
    public void ToResponseListFromEntityList()
    {
        var entities = new List<ProductoEntity>
        {
            new ProductoEntity
            {
                Id = 1,
                Nombre = "Product 1",
                Descripcion = "Description 1",
                Tae = 5.5,
                TipoProducto = "Type 1",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new ProductoEntity
            {
                Id = 2,
                Nombre = "Product 2",
                Descripcion = "Description 2",
                Tae = 6.5,
                TipoProducto = "Type 2",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true
            }
        };

        var results = ProductoMapper.ToResponseListFromEntityList(entities).ToList();

        Assert.That(results, Is.Not.Null);
        Assert.That(2, Is.EqualTo(results.Count));

        for (int i = 0; i < entities.Count; i++)
        {
            Assert.That(entities[i].Nombre, Is.EqualTo(results[i].Nombre));
            Assert.That(entities[i].Descripcion, Is.EqualTo(results[i].Descripcion));
            Assert.That(entities[i].Tae, Is.EqualTo(results[i].Tae));
            Assert.That(entities[i].TipoProducto, Is.EqualTo(results[i].TipoProducto));
            
            var formattedCreatedAt = entities[i].CreatedAt.ToString();
            Assert.That(formattedCreatedAt, Is.EqualTo(results[i].CreatedAt));

            var formattedUpdatedAt = entities[i].UpdatedAt.ToString();
            Assert.That(formattedUpdatedAt, Is.EqualTo(results[i].UpdatedAt));
            
            Assert.That(entities[i].IsDeleted, Is.EqualTo(results[i].IsDeleted));
        }
    }
}