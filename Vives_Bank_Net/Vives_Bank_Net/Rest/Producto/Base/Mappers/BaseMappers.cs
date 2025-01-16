using System.Globalization;
using Vives_Bank_Net.Rest.Producto.Base.Database;

namespace DefaultNamespace;

public static class BaseMappers
{

    public static BaseEntity ToEntityFromModel(this BaseModel model)
    {
        return new BaseEntity
        {
            Id = model.Id,
            Guid = model.Guid,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };

    }

    public static BaseModel ToModelFromEntity(this BaseEntity entity)
    {
        return new BaseModel
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }

    public static BaseEntity ToEntityFromRequest(this BaseRequestDto dto)
    {
        return new BaseEntity
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Tae = dto.Tae,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };
    }

    public static BaseResponseDto ToResponseFromEntity(this BaseEntity entity)
    {
        return new BaseResponseDto
        {
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            Tae = entity.Tae,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }

    public static BaseResponseDto ToResponseFromModel(this BaseModel model)
    {
        return new BaseResponseDto
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Tae = model.Tae,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };
    }
    
    public static List<BaseModel> ToModelList(this List<BaseEntity> entities)
    {
        return entities.Select(entity => entity.ToModelFromEntity()).ToList();
    }
    
}