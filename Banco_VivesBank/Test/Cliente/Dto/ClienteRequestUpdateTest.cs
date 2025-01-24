using Banco_VivesBank.Cliente.Dto;

namespace Test.Cliente.Dto;

[TestFixture]
public class ClienteRequestUpdateTest
{
    
    [Test]
    public void HasAtLeastOneField_ShouldReturnFalse_WhenAllFieldsAreNullOrWhitespace()
    {
        // Arrange
        var clienteRequestUpdate = new ClienteRequestUpdate();

        // Act
        var result = clienteRequestUpdate.HasAtLeastOneField();

        // Assert
        Assert.That(result, Is.False);
    }

    [TestCase("John", null, null, null, null, null, null, null, null)]
    [TestCase(null, "Doe", null, null, null, null, null, null, null)]
    [TestCase(null, null, "Calle Falsa", null, null, null, null, null, null)]
    [TestCase(null, null, null, "123", null, null, null, null, null)]
    [TestCase(null, null, null, null, "28001", null, null, null, null)]
    [TestCase(null, null, null, null, null, "1A", null, null, null)]
    [TestCase(null, null, null, null, null, null, "B", null, null)]
    [TestCase(null, null, null, null, null, null, null, "email@example.com", null)]
    [TestCase(null, null, null, null, null, null, null, null, "612345678")]
    public void HasAtLeastOneField_ShouldReturnTrue_WhenAtLeastOneFieldIsNotNullOrWhitespace(
        string? nombre,
        string? apellidos,
        string? calle,
        string? numero,
        string? codigoPostal,
        string? piso,
        string? letra,
        string? email,
        string? telefono)
    {
        // Arrange
        var clienteRequestUpdate = new ClienteRequestUpdate
        {
            Nombre = nombre,
            Apellidos = apellidos,
            Calle = calle,
            Numero = numero,
            CodigoPostal = codigoPostal,
            Piso = piso,
            Letra = letra,
            Email = email,
            Telefono = telefono
        };

        // Act
        var result = clienteRequestUpdate.HasAtLeastOneField();

        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void HasAtLeastOneField_ShouldReturnTrue_WhenMultipleFieldsAreNotNullOrWhitespace()
    {
        // Arrange
        var clienteRequestUpdate = new ClienteRequestUpdate
        {
            Nombre = "John",
            Apellidos = "Doe",
            Email = "email@example.com"
        };

        // Act
        var result = clienteRequestUpdate.HasAtLeastOneField();

        // Assert
        Assert.That(result, Is.True);
    }

}