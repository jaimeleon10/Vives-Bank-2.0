using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;

namespace Test.Movimientos.Mapper;

[TestFixture]
public class IngresoNominaMapperTests
{
    [Test]
    public void ToResponseFromModel()
    {
        var ingresoNomina = new IngresoNomina
        {
            NombreEmpresa = "Empresa S.A.",
            CifEmpresa = "A12345678",
            IbanEmpresa = "ES9121000418450200051332",
            IbanCliente = "ES9121000418450200051333",
            Importe = 1500.75
        };
        
        var result = ingresoNomina.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NombreEmpresa, Is.EqualTo(ingresoNomina.NombreEmpresa));
        Assert.That(result.CifEmpresa, Is.EqualTo(ingresoNomina.CifEmpresa));
        Assert.That(result.IbanEmpresa, Is.EqualTo(ingresoNomina.IbanEmpresa));
        Assert.That(result.IbanCliente, Is.EqualTo(ingresoNomina.IbanCliente));
        Assert.That(result.Importe, Is.EqualTo(ingresoNomina.Importe));
    }

    [Test]
    public void ToResponseFromModel_TodoNull()
    {
        IngresoNomina? ingresoNomina = null;
        var result = ingresoNomina.ToResponseFromModel();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToResponseFromModel_AlgoNull()
    {
        var ingresoNomina = new IngresoNomina
        {
            NombreEmpresa = null,
            CifEmpresa = null,
            IbanEmpresa = null,
            IbanCliente = null,
            Importe = 0
        };
        
        var result = ingresoNomina.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);  
        Assert.That(result.NombreEmpresa, Is.Null);
        Assert.That(result.CifEmpresa, Is.Null);
        Assert.That(result.IbanEmpresa, Is.Null);
        Assert.That(result.IbanCliente, Is.Null);
        Assert.That(result.Importe, Is.EqualTo(0)); 
    }
}