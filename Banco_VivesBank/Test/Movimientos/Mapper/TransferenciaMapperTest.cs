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
            ClienteOrigen = "Cliente A",
            IbanOrigen = "ES1234567890123456789012",
            NombreBeneficiario = "Juan Perez",
            IbanDestino = "ES9876543210987654321098",
            Importe = 1000.50,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ClienteOrigen, Is.EqualTo(transferencia.ClienteOrigen));
        Assert.That(result.IbanOrigen, Is.EqualTo(transferencia.IbanOrigen));
        Assert.That(result.NombreBeneficiario, Is.EqualTo(transferencia.NombreBeneficiario));
        Assert.That(result.IbanDestino, Is.EqualTo(transferencia.IbanDestino));
        Assert.That(result.Importe, Is.EqualTo(transferencia.Importe));
        Assert.That(result.Revocada, Is.EqualTo(transferencia.Revocada));
    }

    [Test]
    public void ToResponseFromModel_Vacio()
    {
        Transferencia? transferencia = null;
        var result = transferencia.ToResponseFromModel();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToResponseFromModel_DebeManejarPropiedadesVacias()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = string.Empty,
            IbanOrigen = string.Empty,
            NombreBeneficiario = string.Empty,
            IbanDestino = string.Empty,
            Importe = 0,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ClienteOrigen, Is.EqualTo(string.Empty));
        Assert.That(result.IbanOrigen, Is.EqualTo(string.Empty));
        Assert.That(result.NombreBeneficiario, Is.EqualTo(string.Empty));
        Assert.That(result.IbanDestino, Is.EqualTo(string.Empty));
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.Revocada, Is.False);
    }

    [Test]
    public void ToResponseFromModel_ConNulos()
    {
        var transferencia = new Transferencia
        {
            ClienteOrigen = null,
            IbanOrigen = null,
            NombreBeneficiario = null,
            IbanDestino = null,
            Importe = 0,
            Revocada = false
        };
        
        var result = transferencia.ToResponseFromModel();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ClienteOrigen, Is.Null);
        Assert.That(result.IbanOrigen, Is.Null);
        Assert.That(result.NombreBeneficiario, Is.Null);
        Assert.That(result.IbanDestino, Is.Null);
        Assert.That(result.Importe, Is.EqualTo(0));
        Assert.That(result.Revocada, Is.False);
    }
}
