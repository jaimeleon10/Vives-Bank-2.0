using Vives_Bank_Net.Rest.Producto.Cuenta.Database;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Mappers;

public static class CuentaMapper
{
    public static CuentaEntity ToCuentaEntity(this Cuenta cuenta)
    {
        return new CuentaEntity
        {
            Id = cuenta.Id,
            Guid = cuenta.Guid,
            Iban = cuenta.Iban,
            Saldo = cuenta.Saldo,
            TipoCuenta = cuenta.TipoCuenta,
            TarjetaId = cuenta.TarjetaId,
            ClienteId = cuenta.ClienteId,
            ProductoId = cuenta.ProductoId,
            IsDeleted = cuenta.IsDeleted,
            CreatedAt = cuenta.CreatedAt,
            UpdatedAt = cuenta.UpdatedAt
            
        };
    }

    public static Cuenta toCuenta(this CuentaEntity cuentaEntity)
    {
        return new Cuenta
        {
            Id = cuentaEntity.Id,
            Guid = cuentaEntity.Guid,
            Iban = cuentaEntity.Iban,
            Saldo = cuentaEntity.Saldo,
            TipoCuenta = cuentaEntity.TipoCuenta,
            TarjetaId = cuentaEntity.TarjetaId,
            ClienteId = cuentaEntity.ClienteId,
            ProductoId = cuentaEntity.ProductoId,
            IsDeleted = cuentaEntity.IsDeleted,
            CreatedAt = cuentaEntity.CreatedAt,
            UpdatedAt = cuentaEntity.UpdatedAt
        };
    }
}