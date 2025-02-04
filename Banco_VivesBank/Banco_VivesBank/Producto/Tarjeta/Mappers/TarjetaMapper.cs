using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Tarjeta.Mappers;

/// <summary>
/// Proporciona métodos de extensión para mapear entre entidades, modelos y DTOs de tarjetas.
/// </summary>
public static class TarjetaMapper
{
    /// <summary>
    /// Convierte un modelo de tarjeta en una entidad de base de datos.
    /// </summary>
    /// <param name="model">Modelo de tarjeta.</param>
    /// <returns>Entidad de tarjeta.</returns>
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

    
    /// <summary>
    /// Convierte un DTO de solicitud de tarjeta en un modelo de tarjeta.
    /// </summary>
    /// <param name="dto">DTO de solicitud de tarjeta.</param>
    /// <returns>Modelo de tarjeta.</returns>
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
    
    
    /// <summary>
    /// Convierte un DTO de actualización de tarjeta en un modelo de tarjeta.
    /// </summary>
    /// <param name="dto">DTO de actualización de tarjeta.</param>
    /// <returns>Modelo de tarjeta.</returns>
    public static Models.Tarjeta ToModelFromRequestUpdate(this TarjetaRequestUpdate dto)
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

    
    /// <summary>
    /// Convierte una entidad de tarjeta en un modelo de tarjeta.
    /// </summary>
    /// <param name="entity">Entidad de tarjeta.</param>
    /// <returns>Modelo de tarjeta.</returns>
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

    
    /// <summary>
    /// Convierte una entidad de tarjeta en un DTO de respuesta de tarjeta.
    /// </summary>
    /// <param name="tarjetaEntity">Entidad de tarjeta.</param>
    /// <returns>DTO de respuesta de tarjeta.</returns>
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

    
    /// <summary>
    /// Convierte un modelo de tarjeta en un DTO de respuesta de tarjeta.
    /// </summary>
    /// <param name="tarjeta">Modelo de tarjeta.</param>
    /// <returns>DTO de respuesta de tarjeta.</returns>
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
    
    
    /// <summary>
    /// Convierte una lista de entidades de tarjeta en una lista de DTOs de respuesta de tarjeta.
    /// </summary>
    /// <param name="entities">Lista de entidades de tarjeta.</param>
    /// <returns>Lista de DTOs de respuesta de tarjeta.</returns>
    public static List<TarjetaResponse> ToResponseList(this List<TarjetaEntity> entities)
    {
        return entities.Select(entity => entity.ToResponseFromEntity()).ToList();
    }
}