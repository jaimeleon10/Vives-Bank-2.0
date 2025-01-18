using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Utils.Generators;
using NUnit.Framework;

namespace Banco_VivesBank.Test.Producto.Cuenta;

[TestFixture]
public class CuentaMapperTest
{
    [Test]
    public void ToCuentaEntity()
    {
        var cuenta = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta()
        {
            Guid = GuidGenerator.GenerarId(),
            Iban = IbanGenerator.GenerateIban(),
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 2,
            ProductoId = 3,
            IsDeleted = false
        };

        var result = cuenta.ToCuentaEntity();
        Assert.That(result.Guid, Is.EqualTo(cuenta.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuenta.Iban));

    }

    [Test]
    public void ToCuentaResponse_MapsCorrectly()
    {
        var cuentaEntity = new CuentaEntity
        {
            Guid = GuidGenerator.GenerarId(),
            Iban = IbanGenerator.GenerateIban(),
            Saldo = 1500,
            TarjetaId = 4,
            ClienteId = 5,
            ProductoId = 6,
            IsDeleted = true
        };

        var result = cuentaEntity.ToCuentaResponse();

        Assert.That(result.Guid, Is.EqualTo(cuentaEntity.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaEntity.Iban));
    }
}