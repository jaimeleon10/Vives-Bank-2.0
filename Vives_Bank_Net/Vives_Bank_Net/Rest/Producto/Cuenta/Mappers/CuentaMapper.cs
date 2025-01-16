using Vives_Bank_Net.Rest.Producto.Cuenta.Database;
using Vives_Bank_Net.Rest.Producto.Cuenta.Dto;
using Vives_Bank_Net.Rest.Producto.Cuenta.Exceptions;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Mappers;

public static class CuentaMapper
{
    public static CuentaEntity ToCuentaEntity(this Models.Cuenta cuenta)
    {
        return new CuentaEntity
        {
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TarjetaId = cuenta.TarjetaId,
            ClienteId = cuenta.ClienteId,
            ProductoId = cuenta.ProductoId,
            IsDeleted = cuenta.IsDeleted
        };
    }
    
    public static CuentaResponse ToCuentaResponse(this CuentaEntity cuentaEntity)
    {
        return new CuentaResponse
        {
            Guid = cuentaEntity.Guid,
            Iban = cuentaEntity.Iban,
            Saldo = cuentaEntity.Saldo,
            TarjetaId = cuentaEntity.TarjetaId,
            ClienteId = cuentaEntity.ClienteId,
            ProductoId = cuentaEntity.ProductoId
        };
    }
}