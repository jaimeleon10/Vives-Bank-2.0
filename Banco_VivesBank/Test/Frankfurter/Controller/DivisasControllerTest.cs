using Banco_VivesBank.Frankfurter.Controller;
using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Model;
using Banco_VivesBank.Frankfurter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Frankfurter.Controller;

public class DivisasControllerTest
{
    private Mock<ILogger<DivisasController>> _logger;
    private Mock<IDivisasService> _divisasService;
    private DivisasController _controller;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<DivisasController>>();
        _divisasService = new Mock<IDivisasService>();
        _controller = new DivisasController(_divisasService.Object, _logger.Object);
    }

    [Test]
    public void ObtenerCambio()
    {
        var respuestaEsperada = new FrankFurterResponse 
        { 
            Amount = 1,
            Base = "EUR",
            Rates = new Dictionary<string, decimal> { { "USD", 1.13m } }
        };
    
        _divisasService
            .Setup(x => x.ObtenerUltimasTasas(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(respuestaEsperada);

        var result = _controller.GetLatestRates("1", "EUR", "USD");

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(respuestaEsperada));
    }

    [Test]
    public void ObtenerCambios()
    {
        var respuestaEsperada = new FrankFurterResponse
        {
            Amount = 1,
            Base = "EUR",
            Rates = new Dictionary<string, decimal> { { "USD", 1.13m } }
        };

        _divisasService
            .Setup(x => x.ObtenerUltimasTasas("EUR", null, "1"))
            .Returns(respuestaEsperada);

        var result = _controller.GetLatestRates();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        _divisasService.Verify(x => x.ObtenerUltimasTasas("EUR", null, "1"), Times.Once);
    }
    
    [Test]
    public void GetLatestRatesError()
    {
        var baseCurrency = "EUR";
        var symbol = "USD";
        var amount = "1";
        var exceptionMessage = "Error de conexión";

        var expectedException = new Exception(exceptionMessage);

        _divisasService
            .Setup(x => x.ObtenerUltimasTasas(baseCurrency, symbol, amount))
            .Throws(expectedException);

        var ex = Assert.Throws<FrankFurterConnectionException>(() => _controller.GetLatestRates(amount, baseCurrency, symbol));

        Assert.That(ex.Message, Is.EqualTo($"Error de conexión al obtener las tasas de cambio de {baseCurrency} a {symbol}."));
        Assert.That(ex.InnerException, Is.EqualTo(expectedException));
    
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al obtener las últimas tasas de cambio.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
    
    [Test]
    public void GetLatestRatesParametrosPorDefecto()
    {
        var respuestaEsperada = new FrankFurterResponse 
        { 
            Amount = 1,
            Base = "EUR",
            Rates = new Dictionary<string, decimal> { { "USD", 1.13m } }
        };

        _divisasService
            .Setup(x => x.ObtenerUltimasTasas("EUR", null, "1"))
            .Returns(respuestaEsperada);

        var result = _controller.GetLatestRates();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(respuestaEsperada));

        _divisasService.Verify(x => x.ObtenerUltimasTasas("EUR", null, "1"), Times.Once);
    }
    
    [Test]
    public void GetLatestRatesCustomParameters()
    {
        // Arrange
        var baseCurrency = "USD";
        var symbol = "GBP";
        var amount = "100";
        var respuestaEsperada = new FrankFurterResponse 
        { 
            Amount = 100,
            Base = "USD",
            Rates = new Dictionary<string, decimal> { { "GBP", 0.85m } }
        };

        _divisasService
            .Setup(x => x.ObtenerUltimasTasas(baseCurrency, symbol, amount))
            .Returns(respuestaEsperada);

        // Act
        var result = _controller.GetLatestRates(amount, baseCurrency, symbol);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(respuestaEsperada));

        _divisasService.Verify(x => x.ObtenerUltimasTasas(baseCurrency, symbol, amount), Times.Once);
    }
    
    [Test]
    public void GetLatestRatesLogInformation()
    {
        // Arrange
        var baseCurrency = "EUR";
        var symbol = "USD";
        var amount = "1";
        var respuestaEsperada = new FrankFurterResponse 
        { 
            Amount = 1,
            Base = "EUR",
            Rates = new Dictionary<string, decimal> { { "USD", 1.13m } }
        };

        _divisasService
            .Setup(x => x.ObtenerUltimasTasas(baseCurrency, symbol, amount))
            .Returns(respuestaEsperada);

        // Act
        var result = _controller.GetLatestRates(amount, baseCurrency, symbol);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Obteniendo las últimas tasas de cambio desde EUR a USD")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Respuesta construida:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}