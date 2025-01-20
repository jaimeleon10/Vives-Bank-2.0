using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Mappers;

public class BaseMapper
{
    public static BaseModel ToModelFromRequest(BaseRequest d)
    {
        return new BaseModel
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

    public static BaseModel ToModelFromEntity(BaseEntity entity)
    {
        return new BaseModel
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            TipoProducto = entity.TipoProducto,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static BaseEntity ToEntityFromModel(BaseModel model)
    {
        return new BaseEntity
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            TipoProducto = model.TipoProducto,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };

    }

    public static BaseResponse ToResponseFromModel(BaseModel model)
    {
        return new BaseResponse
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            TipoProducto = model.TipoProducto,
            CreatedAt = model.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = model.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = model.IsDeleted
        };
    }
    
    public static BaseResponse ToResponseFromEntity(BaseEntity entity)
    {
        return new BaseResponse
        {
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            TipoProducto = entity.TipoProducto,
            CreatedAt = entity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = entity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static IEnumerable<BaseResponse> ToResponseListFromEntityList(IEnumerable<BaseEntity> entities)
    {
        return entities.Select(entity => ToResponseFromEntity(entity));
    }
    
}