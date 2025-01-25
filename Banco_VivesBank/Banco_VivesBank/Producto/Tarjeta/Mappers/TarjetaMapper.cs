using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Tarjeta.Mappers;

public static class TarjetaMapper
{
    public static TarjetaEntity ToEntityFromModel(this Models.Tarjeta model)
    {
        return new TarjetaEntity
        {
            Id = model.Id,
            Guid = model.Guid,
            Numero = model.Numero,
            FechaVencimiento = model.FechaVencimiento,
            Cvv = model.Cvv,
            Pin = model.Pin,
            LimiteDiario = model.LimiteDiario,
            LimiteSemanal = model.LimiteSemanal,
            LimiteMensual = model.LimiteMensual,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static Models.Tarjeta ToModelFromRequest(this TarjetaRequest dto)
    {
        return new Models.Tarjeta
        {
            Pin = dto.Pin,
            LimiteDiario = dto.LimiteDiario,
            LimiteSemanal = dto.LimiteSemanal,
            LimiteMensual = dto.LimiteMensual,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static Models.Tarjeta ToModelFromEntity(this TarjetaEntity entity)
    {
        return new Models.Tarjeta
        {
            Id = entity.Id,
            Guid = entity.Guid,
            Numero = entity.Numero,
            FechaVencimiento = entity.FechaVencimiento,
            Cvv = entity.Cvv,
            Pin = entity.Pin,
            LimiteDiario = entity.LimiteDiario,
            LimiteSemanal = entity.LimiteSemanal,
            LimiteMensual = entity.LimiteMensual,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }

    public static TarjetaResponse ToResponseFromEntity(this TarjetaEntity tarjetaEntity)
    {
        return new TarjetaResponse
        {
            Guid = tarjetaEntity.Guid,
            Numero = tarjetaEntity.Numero,
            FechaVencimiento = tarjetaEntity.FechaVencimiento,
            Cvv = tarjetaEntity.Cvv,
            Pin = tarjetaEntity.Pin,
            LimiteDiario = tarjetaEntity.LimiteDiario,
            LimiteSemanal = tarjetaEntity.LimiteSemanal,
            LimiteMensual = tarjetaEntity.LimiteMensual,
            CreatedAt = tarjetaEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = tarjetaEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = tarjetaEntity.IsDeleted
        };
    }

    public static TarjetaResponse ToResponseFromModel(this Models.Tarjeta tarjeta)
    {
        return new TarjetaResponse
        {
            Guid = tarjeta.Guid,
            Numero = tarjeta.Numero,
            FechaVencimiento = tarjeta.FechaVencimiento,
            Cvv = tarjeta.Cvv,
            Pin = tarjeta.Pin,
            LimiteDiario = tarjeta.LimiteDiario,
            LimiteSemanal = tarjeta.LimiteSemanal,
            LimiteMensual = tarjeta.LimiteMensual,
            CreatedAt = tarjeta.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = tarjeta.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = tarjeta.IsDeleted
        };
    }
    
    public static List<TarjetaResponse> ToResponseList(this List<TarjetaEntity> entities)
    {
        return entities.Select(entity => entity.ToResponseFromEntity()).ToList();
    }
}