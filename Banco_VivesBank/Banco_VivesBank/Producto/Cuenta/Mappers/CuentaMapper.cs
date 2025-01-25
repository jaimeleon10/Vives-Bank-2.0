using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Mappers;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Mappers;

namespace Banco_VivesBank.Producto.Cuenta.Mappers;

public static class CuentaMapper
{
    public static Models.Cuenta ToModelFromEntity(this CuentaEntity entity)
    {
        return new Models.Cuenta
        {
            Guid = entity.Guid,
            Iban = entity.Iban,
            Saldo = entity.Saldo,
            Tarjeta = entity.Tarjeta.ToModelFromEntity(),
            Cliente = entity.Cliente.ToModelFromEntity(),
            Producto = entity.Producto.ToModelFromEntity(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static CuentaEntity ToEntityFromModel(this Models.Cuenta cuenta)
    {
        return new CuentaEntity
        {
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TarjetaId = cuenta.Tarjeta.Id,
            ClienteId = cuenta.Cliente.Id,
            ProductoId = cuenta.Producto.Id,
            CreatedAt = cuenta.CreatedAt,
            UpdatedAt = cuenta.UpdatedAt,
            IsDeleted = cuenta.IsDeleted
        };
    }
    
    public static CuentaResponse ToResponseFromModel(this Models.Cuenta cuenta)
    {
        return new CuentaResponse
        {
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TarjetaGuid = cuenta.Tarjeta.Guid,
            ClienteGuid = cuenta.Cliente.Guid,
            ProductoGuid = cuenta.Producto.Guid,
            CreatedAt = cuenta.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cuenta.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = cuenta.IsDeleted
        };
    }
    
    public static CuentaResponse ToResponseFromEntity(this CuentaEntity cuentaEntity)
    {
        return new CuentaResponse
        {
            Guid = cuentaEntity.Guid,
            Iban = cuentaEntity.Iban,
            Saldo = cuentaEntity.Saldo,
            TarjetaGuid = cuentaEntity.Tarjeta.Guid,
            ClienteGuid = cuentaEntity.Cliente.Guid,
            ProductoGuid = cuentaEntity.Producto.Guid,
            CreatedAt = cuentaEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cuentaEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = cuentaEntity.IsDeleted
        };
    }
}