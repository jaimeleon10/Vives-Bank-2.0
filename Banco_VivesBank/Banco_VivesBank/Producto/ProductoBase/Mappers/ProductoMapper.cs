using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.ProductoBase.Dto;

namespace Banco_VivesBank.Producto.ProductoBase.Mappers;

public static class ProductoMapper
{
    public static ProductoBase.Models.Producto ToModelFromRequest(this ProductoRequest d)
    {
        return new ProductoBase.Models.Producto
        {
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            Tae = d.Tae,
            TipoProducto = d.TipoProducto,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = d.IsDeleted
        };
    }

    public static ProductoBase.Models.Producto ToModelFromEntity(this ProductoEntity entity)
    {
        return new ProductoBase.Models.Producto
        {
            Id = entity.Id,
            Guid = entity.Guid,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            TipoProducto = entity.TipoProducto,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static ProductoEntity ToEntityFromModel(this ProductoBase.Models.Producto model)
    {
        return new ProductoEntity
        {
            Id = model.Id,
            Guid = model.Guid,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            TipoProducto = model.TipoProducto,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };

    }

    public static ProductoResponse ToResponseFromModel(this ProductoBase.Models.Producto model)
    {
        return new ProductoResponse
        {
            Nombre = model.Nombre,
            Guid = model.Guid,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            TipoProducto = model.TipoProducto,
            CreatedAt = model.CreatedAt.ToString(),
            UpdatedAt = model.UpdatedAt.ToString(),
            IsDeleted = model.IsDeleted
        };
    }
    
    public static ProductoResponse ToResponseFromEntity(this ProductoEntity entity)
    {
        return new ProductoResponse
        {
            Nombre = entity.Nombre,
            Guid = entity.Guid,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            TipoProducto = entity.TipoProducto,
            CreatedAt = entity.CreatedAt.ToString(),
            UpdatedAt = entity.UpdatedAt.ToString(),
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static IEnumerable<ProductoResponse> ToResponseListFromEntityList(this IEnumerable<ProductoEntity> entities)
    {
        return entities.Select(entity => ToResponseFromEntity(entity));
    }
    
}