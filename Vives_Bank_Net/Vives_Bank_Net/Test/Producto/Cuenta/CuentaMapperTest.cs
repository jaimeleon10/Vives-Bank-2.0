using NUnit.Framework;
using Vives_Bank_Net.Rest.Producto.Cuenta.Database;
using Vives_Bank_Net.Rest.Producto.Cuenta.Mappers;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Test.Producto.Cuenta;

[TestFixture]
public class CuentaMapperTest
{
    [Test]
    public void ToCuentaEntity()
    {
        var cuenta = new Rest.Producto.Cuenta.Models.Cuenta()
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