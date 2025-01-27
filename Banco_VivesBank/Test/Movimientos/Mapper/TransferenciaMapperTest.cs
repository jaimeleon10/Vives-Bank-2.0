using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;

namespace Test.Movimientos.Mapper;

[TestFixture]
public class TransferenciaMapperTests
{
    [Test]
    public void ToResponseFromModel()
    {
        var transferencia = new Transferencia
        {
            IbanOrigen = "ES1234567890123456789012",
            NombreBeneficiario = "Juan Perez",
            IbanDestino = "ES9876543210987654321098",
            Importe = 1000.50,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IbanOrigen, Is.EqualTo(transferencia.IbanOrigen));
        Assert.That(result.NombreBeneficiario, Is.EqualTo(transferencia.NombreBeneficiario));
        Assert.That(result.IbanDestino, Is.EqualTo(transferencia.IbanDestino));
        Assert.That(result.Importe, Is.EqualTo(transferencia.Importe));
        Assert.That(result.Revocada, Is.EqualTo(transferencia.Revocada));
    }

    [Test]
    public void ToResponseFromModel_ConNull()
    {
        Transferencia? transferencia = null;
        var result = transferencia.ToResponseFromModel();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToResponseFromModel_ConPropiedadesVacias()
    {
        var transferencia = new Transferencia
        {
            IbanOrigen = string.Empty,
            NombreBeneficiario = string.Empty,
            IbanDestino = string.Empty,
            Importe = 0,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IbanOrigen, Is.EqualTo(string.Empty));
        Assert.That(result.NombreBeneficiario, Is.EqualTo(string.Empty));
        Assert.That(result.IbanDestino, Is.EqualTo(string.Empty));
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.Revocada, Is.EqualTo(false));
    }

    [Test]
    public void ToResponseFromModel_ValoresPorDefecto()
    {
        var transferencia = new Transferencia
        {
            IbanOrigen = null,
            NombreBeneficiario = null,
            IbanDestino = null,
            Importe = 0,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IbanOrigen, Is.Null);
        Assert.That(result.NombreBeneficiario, Is.Null);
        Assert.That(result.IbanDestino, Is.Null);
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.Revocada, Is.EqualTo(false));
    }
}