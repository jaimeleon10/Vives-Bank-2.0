using System.ComponentModel.DataAnnotations;
using Banco_VivesBank.Utils.Validators;

namespace Test.Utils.Validators;

[TestFixture]
public class DniValidationTest
{
    private  DniValidation _dniValidation;
    
    [SetUp]
    public void SetUp()
    {
        _dniValidation = new DniValidation();
    }
    
    [Test]
    [TestCase("12345678Z")]
    [TestCase("00000000t")]
    public void DniValido(string dni)
    {
        var result = _dniValidation.GetValidationResult(dni, new ValidationContext(new()));
        Assert.That(result, Is.EqualTo(ValidationResult.Success));
    }
    
    [Test]
    [TestCase("ABCDEFGHZ")]  
    public void DniInvalido(string dni)
    {
        var result = _dniValidation.GetValidationResult(dni, new ValidationContext(new()));
        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.EqualTo("El DNI debe tener 8 números seguidos de una letra en mayúsculas"));
    }

    [Test]
    [TestCase("12345678A")]
    public void dniInvalidoLetraIncorrecta(string dni)
    {
        var result = _dniValidation.GetValidationResult(dni, new ValidationContext(new()));
        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result.ErrorMessage, Is.EqualTo("La letra del DNI no es correcta"));
    }
    
    [Test]
    public void DniNull()
    {
        var result = _dniValidation.GetValidationResult(null, new ValidationContext(new()));
        Assert.That(result, Is.EqualTo(ValidationResult.Success));
    }
}