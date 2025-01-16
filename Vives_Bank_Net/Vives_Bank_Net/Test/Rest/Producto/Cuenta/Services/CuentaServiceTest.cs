using Vives_Bank_Net.Rest.Producto.Cuenta;
using Vives_Bank_Net.Rest.Producto.Cuenta.Database;
using Vives_Bank_Net.Rest.Producto.Cuenta.Exceptions;
using Vives_Bank_Net.Rest.Producto.Cuenta.Services;
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;


[TestFixture]
public class CuentaServiceTests
{
    private Mock<CuentaDbContext> _mockDbContext;
    private Mock<ILogger<CuentaService>> _mockLogger;
    private CuentaService _cuentaService;

    [SetUp]
    public void SetUp()
    {
        _mockDbContext = new Mock<CuentaDbContext>();
        _mockLogger = new Mock<ILogger<CuentaService>>();
        _cuentaService = new CuentaService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetByGuid_CuentaExiste_RetornaCuentaResponse()
    {
        // Arrange
        var guid = "test-guid";
        var mockCuenta = new Cuenta { Guid = guid };
        var mockDbSet = CreateMockDbSet(new List<Cuenta> { mockCuenta });
        _mockDbContext.Setup(x => x.Cuentas).Returns(mockDbSet.Object);

        // Act
        var result = await _cuentaService.getByGuid(guid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(guid));
    }

    [Test]
    public async Task GetByGuid_CuentaNoExiste_LanzaExcepcion()
    {
        // Arrange
        var guid = "test-guid";
        var mockDbSet = CreateMockDbSet(new List<Cuenta>());
        _mockDbContext.Setup(x => x.Cuentas).Returns(mockDbSet.Object);

        // Act & Assert
        Assert.ThrowsAsync<CuentaNoEncontradaException>(async () => await _cuentaService.getByGuid(guid));
      
    }

    [Test]
    public async Task GetByIban_CuentaExiste_RetornaCuentaResponse()
    {
        // Arrange
        var iban = "test-iban";
        var mockCuenta = new Cuenta { Iban = iban };
        var mockDbSet = CreateMockDbSet(new List<Cuenta> { mockCuenta });
        _mockDbContext.Setup(x => x.Cuentas).Returns(mockDbSet.Object);

        // Act
        var result = await _cuentaService.getByIban(iban);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(iban, result.Iban);
        _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetByIban_CuentaNoExiste_LanzaExcepcion()
    {
        // Arrange
        var iban = "test-iban";
        var mockDbSet = CreateMockDbSet(new List<Cuenta>());
        _mockDbContext.Setup(x => x.Cuentas).Returns(mockDbSet.Object);

        // Act & Assert
        Assert.ThrowsAsync<CuentaNoEncontradaException>(async () => await _cuentaService.getByIban(iban));
        _mockLogger.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

        return mockDbSet;
    }
}