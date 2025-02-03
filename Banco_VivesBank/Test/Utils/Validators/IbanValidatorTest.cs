using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Test.Utils.Validators;

public class IbanValidatorTest
{
    private IbanValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new IbanValidator();
    }

    [Test]
    [TestCase("ES9121000418450200051332")] // IBAN válido de España
    [TestCase("DE89370400440532013000")] // IBAN válido de Alemania
    [TestCase("FR1420041010050500013M02606")] // IBAN válido de Francia
    public void Valido(string iban)
    {
        var result = _validator.GetValidationResult(iban, new ValidationContext(new { }));

        Assert.That(result, Is.EqualTo(ValidationResult.Success));
    }

    [Test]
    [TestCase("ES9121000418450200051333")] // no cumple módulo 97
    [TestCase("GB82 WEST 1234 5698 7654 32")] // Contiene espacios
    [TestCase("ES91-2100-0418-4502-0005-1332")] // Contiene guiones
    [TestCase("12345678901234567890")] // No tiene prefijo de país
    [TestCase("XX0012345678901234567890")] // Código de país inválido
    public void Invalido_IbanErroneo(string iban)
    {
        var result = _validator.GetValidationResult(iban, new ValidationContext(new { }));

        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    [TestCase("")]
    public void Invalido_Empty(string iban)
    {
        var result = _validator.GetValidationResult(iban, new ValidationContext(new { }));

        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.EqualTo("El IBAN es un campo obligatorio"));
    }
    
    [Test]
    public void Invalido_Null()
    {
        var result = _validator.GetValidationResult(null, new ValidationContext(new { }));

        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.EqualTo("El IBAN es un campo obligatorio"));
    }
    
}