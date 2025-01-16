using Vives_Bank_Net.Rest.Producto.Base.Database;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Database;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Dto;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Models;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Mappers;

public static class TarjetaMappers
{
    public static TarjetaEntity ToEntityFromModel(this Models.Tarjeta model)
    {
        return new TarjetaEntity
        {
            Id = model.Id,
            Guid = model.Guid,
            Numero = model.Numero,
            Titular = model.Titular,
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

    public static Models.Tarjeta ToModelFromRequest(this TarjetaRequestDto dto)
    {
        return new Models.Tarjeta
        {
            Pin = dto.Pin,
            LimiteDiario = dto.LimiteDiario,
            LimiteSemanal = dto.LimiteSemanal,
            LimiteMensual = dto.LimiteMensual,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
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
            Titular = entity.Titular,
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

    public static TarjetaResponseDto ToResponseFromEntity(this TarjetaEntity entity)
    {
        return new TarjetaResponseDto
        {
            Id = entity.Id,
            Guid = entity.Guid,
            Numero = entity.Numero,
            Titular = entity.Titular,
            FxVencimiento = entity.FechaVencimiento,
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

    public static TarjetaResponseDto ToResponseFromModel(this Models.Tarjeta model)
    {
        return new TarjetaResponseDto
        {
            Id = model.Id,
            Guid = model.Guid,
            Numero = model.Numero,
            Titular = model.Titular,
            FxVencimiento = model.FechaVencimiento,
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
    
    public static List<Models.Tarjeta> ToModelList(this List<TarjetaEntity> entities)
    {
        return entities.Select(entity => entity.ToModelFromEntity()).ToList();
    }
}