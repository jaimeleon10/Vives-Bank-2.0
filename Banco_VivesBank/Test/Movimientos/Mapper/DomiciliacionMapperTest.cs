using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;

namespace Test.Movimientos.Mapper;

[TestFixture]
public class DomiciliacionMapperTests
{
    [Test]
    public void ToResponseFromModel_ConNull()
    {
        Domiciliacion? input = null;
        var result = DomiciliacionMapper.ToResponseFromModel(input);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToResponseFromModel()
    {
        var input = new Domiciliacion
        {
            Id = "645db8c5f4321b7e90f40345",
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = "Empresa XYZ",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES0987654321098765432109",
            Importe = 1234.56,
            Periodicidad = Periodicidad.Mensual,
            Activa = true,
            FechaInicio = new DateTime(2023, 1, 1, 14, 30, 0),
            UltimaEjecucion = new DateTime(2023, 1, 15, 10, 0, 0)
        };

        var result = DomiciliacionMapper.ToResponseFromModel(input);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(input.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(input.ClienteGuid));
        Assert.That(result.Acreedor, Is.EqualTo(input.Acreedor));
        Assert.That(result.IbanEmpresa, Is.EqualTo(input.IbanEmpresa));
        Assert.That(result.IbanCliente, Is.EqualTo(input.IbanCliente));
        Assert.That(result.Importe, Is.EqualTo(input.Importe));
        Assert.That(result.Periodicidad, Is.EqualTo(input.Periodicidad.ToString()));
        Assert.That(result.Activa, Is.EqualTo(input.Activa));
        
        Assert.That(result.FechaInicio, Is.EqualTo("01/01/2023 14:30:00"));
        Assert.That(result.UltimaEjecuccion, Is.EqualTo("15/01/2023 10:00:00"));
    }

    [Test]
    public void ToResponseFromModel_AlgunosCamposNulos()
    {
        var input = new Domiciliacion
        {
            Id = "645db8c5f4321b7e90f40345",
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = null,
            IbanEmpresa = null,
            IbanCliente = "ES0987654321098765432109",
            Importe = 567.89,
            Periodicidad = Periodicidad.Anual,
            Activa = false,
            FechaInicio = new DateTime(2023, 6, 1, 9, 0, 0),
            UltimaEjecucion = new DateTime(2023, 6, 30, 17, 0, 0)
        };

        var result = DomiciliacionMapper.ToResponseFromModel(input);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Acreedor, Is.Null);
        Assert.That(result.IbanEmpresa, Is.Null);
        Assert.That(result.IbanCliente, Is.EqualTo(input.IbanCliente));
        Assert.That(result.Importe, Is.EqualTo(input.Importe));
        Assert.That(result.Periodicidad, Is.EqualTo(input.Periodicidad.ToString()));
        Assert.That(result.Activa, Is.EqualTo(input.Activa));
        
        Assert.That(result.FechaInicio, Is.EqualTo("01/06/2023 9:00:00"));
        Assert.That(result.UltimaEjecuccion, Is.EqualTo("30/06/2023 17:00:00"));
    }
}