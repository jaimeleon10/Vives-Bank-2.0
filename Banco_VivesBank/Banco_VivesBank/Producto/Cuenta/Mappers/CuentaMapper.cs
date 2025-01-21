using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Cuenta.Mappers;

public static class CuentaMapper
{
    /*public static Models.Cuenta ToModelFromEntity(CuentaEntity entity)
    {
        return new Models.Cuenta
        {
            Guid = entity.Guid,
            Iban = entity.Iban,
            Saldo = entity.Saldo,
            Tarjeta = entity.TarjetaId,
            Cliente = entity.ClienteId,
            Producto = entity.ProductoId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }
    
    public static CuentaEntity ToEntityFromModel(Models.Cuenta cuenta)
    {
        return new CuentaEntity
        {
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TarjetaId = cuenta.Tarjeta,
            ClienteId = cuenta.Cliente,
            ProductoId = cuenta.Producto,
            CreatedAt = cuenta.CreatedAt,
            UpdatedAt = cuenta.UpdatedAt,
            IsDeleted = cuenta.IsDeleted
        };
    }*/
    
    public static CuentaResponse ToResponseFromModel(Models.Cuenta cuenta, string tarjetaGuid, string clienteGuid, string productoGuid)
    {
        return new CuentaResponse
        {
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TarjetaGuid = tarjetaGuid,
            ClienteGuid = clienteGuid,
            ProductoGuid = productoGuid,
            CreatedAt = cuenta.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cuenta.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = cuenta.IsDeleted
        };
    }
    
    public static CuentaResponse ToResponseFromEntity(CuentaEntity cuentaEntity, Tarjeta.Models.TarjetaModel tarjeta, Cliente.Models.Cliente cliente, BaseModel producto)
    {
        return new CuentaResponse
        {
            Guid = cuentaEntity.Guid,
            Iban = cuentaEntity.Iban,
            Saldo = cuentaEntity.Saldo,
            TarjetaGuid = tarjeta.Guid,
            ClienteGuid = cliente.Guid,
            ProductoGuid = producto.Guid,
            CreatedAt = cuentaEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cuentaEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = cuentaEntity.IsDeleted
        };
    }
}