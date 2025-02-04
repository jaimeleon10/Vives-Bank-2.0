using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Test.Utils.Validators;

public class CreditCardValidationTest
{
    private CreditCardValidation _validador;

    [SetUp]
    public void Setup()
    {
        _validador = new CreditCardValidation();
    }

    [Test]
    [TestCase("4539578763621486")] 
    [TestCase("6011514433546201")] 
    [TestCase("5500005555555559")] 
    public void IsValid(string cardNumber)
    {
        var result = _validador.GetValidationResult(cardNumber, new ValidationContext(new { }));

        Assert.That(result, Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    [TestCase("4539578763621487")]
    [TestCase("1234567890123456")] 
    [TestCase("ABCDE12345678901")] 
    public void InvalidCardNumber(string cardNumber)
    {
        var result = _validador.GetValidationResult(cardNumber, new ValidationContext(new { }));

        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    [TestCase("")]
 
    public void Invalido_Empty(string cardNumber)
    {
        var result = _validador.GetValidationResult(cardNumber, new ValidationContext(new { }));
        
        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.EqualTo("El número de tarjeta es un campo obligatorio"));
    }

    [Test]
    public void Invalido_Null()
    {
        var result = _validador.GetValidationResult(null, new ValidationContext(new { }));
        
        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
    }
}