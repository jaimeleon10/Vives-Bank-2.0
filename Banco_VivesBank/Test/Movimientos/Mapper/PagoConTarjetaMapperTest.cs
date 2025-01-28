using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;

namespace Test.Movimientos.Mapper;

[TestFixture]
public class PagoConTarjetaMapperTests
{
    [Test]
    public void ToResponseFromModel()
    {
        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = "Comercio Ejemplo",
            Importe = 150.75,
            NumeroTarjeta = "1234567812345678"
        };
        
        var result = pagoConTarjeta.ToResponseFromModel();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.NombreComercio, Is.EqualTo(pagoConTarjeta.NombreComercio));
        Assert.That(result.Importe, Is.EqualTo(pagoConTarjeta.Importe));
        Assert.That(result.NumeroTarjeta, Is.EqualTo(pagoConTarjeta.NumeroTarjeta));
    }

    [Test]
    public void ToResponseFromModel_ConNull()
    {
        PagoConTarjeta? pagoConTarjeta = null;
        var result = pagoConTarjeta.ToResponseFromModel();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToResponseFromModel_ConCamposVacios()
    {
        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = string.Empty,
            Importe = 0,
            NumeroTarjeta = string.Empty
        };
        
        var result = pagoConTarjeta.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NombreComercio, Is.EqualTo(string.Empty));
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.NumeroTarjeta, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToResponseFromModel_ConValoresPorDefecto()
    {
        var pagoConTarjeta = new PagoConTarjeta
        {
            NombreComercio = null,
            Importe = 0,
            NumeroTarjeta = null
        };
        
        var result = pagoConTarjeta.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NombreComercio, Is.Null);
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.NumeroTarjeta, Is.Null);
    }
}