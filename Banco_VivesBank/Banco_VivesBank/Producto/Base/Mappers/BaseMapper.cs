using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Mappers;

public static class BaseMapper
{
    public static Models.Base ToModelFromRequest(this BaseRequest d)
    {
        return new Models.Base
        {
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            Tae = d.Tae,
            TipoProducto = d.TipoProducto,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static Models.Base ToModelFromEntity(this ProductoEntity entity)
    {
        return new Models.Base
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
    
    public static ProductoEntity ToEntityFromModel(this Models.Base model)
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

    public static BaseResponse ToResponseFromModel(this Models.Base model)
    {
        return new BaseResponse
        {
            Nombre = model.Nombre,
            Guid = model.Guid,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            TipoProducto = model.TipoProducto,
            CreatedAt = model.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = model.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = model.IsDeleted
        };
    }
    
    public static BaseResponse ToResponseFromEntity(this ProductoEntity entity)
    {
        return new BaseResponse
        {
            Nombre = entity.Nombre,
            Guid = entity.Guid,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            TipoProducto = entity.TipoProducto,
            CreatedAt = entity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = entity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static IEnumerable<BaseResponse> ToResponseListFromEntityList(this IEnumerable<ProductoEntity> entities)
    {
        return entities.Select(entity => ToResponseFromEntity(entity));
    }
    
}