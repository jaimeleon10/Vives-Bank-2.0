using System.Net;
using System.Text;
using System.Text.Json;
using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Services;
using Moq;
using Moq.Protected;

namespace Test.Frankfurter.Services;

public class DivisasServiceTest
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private DivisasService _divisasService;

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _divisasService = new DivisasService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }


    [Test]
    public void ObtenerCambioException()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        Assert.Throws<FrankFurterUnexpectedException>(() => 
            _divisasService.ObtenerUltimasTasas("EUR", "USD", "1"));
    }

    [Test]
    public void ObtenerUCambio()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13},\"amount\":1,\"base\":\"EUR\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var expectedUri = "https://api.frankfurter.app/latest?base=EUR&symbols=USD&amount=1";
        HttpRequestMessage capturedRequest = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
            .ReturnsAsync(httpResponseMessage);

        _divisasService.ObtenerUltimasTasas("EUR", "USD", "1");

        Assert.That(capturedRequest.RequestUri.ToString(), Is.EqualTo(expectedUri));
    }

    [Test]
    public void ObtenerUltimasTasasVariosCambios()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13,\"GBP\":0.88,\"JPY\":130.45},\"amount\":1,\"base\":\"EUR\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var result = _divisasService.ObtenerUltimasTasas("EUR", "USD,GBP,JPY", "1");

        Assert.That(result.Rates.Count, Is.EqualTo(3));
        Assert.That(result.Rates["USD"], Is.EqualTo(1.13m));
        Assert.That(result.Rates["GBP"], Is.EqualTo(0.88m));
        Assert.That(result.Rates["JPY"], Is.EqualTo(130.45m));
    }
    
    [Test]
    public void ObtenerUltimasTasasEmptyResponseException()
    {
        var jsonResponse = "{}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var ex = Assert.Throws<FrankfurterEmptyResponseException>(() =>
            _divisasService.ObtenerUltimasTasas("EUR", "USD", "1"));

        Assert.That(ex.Message, Is.EqualTo($"No se obtuvieron datos en la respuesta de FrankFurter para la moneda 'EUR', símbolo 'USD' y cantidad '1'."));
    }
    
    [Test]
    public void ObtenerUltimasTasasValidDeserializedObject()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13},\"amount\":1,\"base\":\"EUR\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var result = _divisasService.ObtenerUltimasTasas("EUR", "USD", "1");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Base, Is.EqualTo("EUR"));
        Assert.That(result.Amount, Is.EqualTo(1));
        Assert.That(result.Rates["USD"], Is.EqualTo(1.13m));
    }
    
    [Test]
    public void ObtenerUltimasTasasFrankFurterUnexpectedException()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        var ex = Assert.Throws<FrankFurterUnexpectedException>(() =>
            _divisasService.ObtenerUltimasTasas("EUR", "USD", "1"));

        Assert.That(ex.Message, Does.Contain("Error inesperado al obtener las tasas de cambio de EUR a USD."));
    }
    
    [Test]
    public void ObtenerUltimasTasasThrowsJsonException()
    {
        var invalidJsonResponse = "{\"rates\":{\"USD\":1.13},\"amount\":1,\"base\":\"EUR\"";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invalidJsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);

        Assert.Throws<JsonException>(() =>
            _divisasService.ObtenerUltimasTasas("EUR", "USD", "1"));
    }
    
    [Test]
    public void ObtenerUltimasTasasNullOrEmptyMonedasObjetivo()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13},\"amount\":1,\"base\":\"EUR\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var expectedUri = "https://api.frankfurter.app/latest?base=EUR&amount=1";
        HttpRequestMessage capturedRequest = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
            .ReturnsAsync(httpResponseMessage);

        _divisasService.ObtenerUltimasTasas("EUR", null, "1");

        Assert.That(
            capturedRequest.RequestUri.ToString(),
            Is.EqualTo("https://api.frankfurter.app/latest?base=EUR&symbols=&amount=1")
                .Or.EqualTo("https://api.frankfurter.app/latest?base=EUR&amount=1")
        );
    }
    
    [Test]
    public void ObtenerUltimasTasasNullOrEmptyAmount()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13},\"amount\":1,\"base\":\"EUR\"}";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var expectedUri = "https://api.frankfurter.app/latest?base=EUR&symbols=USD&amount=1";
        HttpRequestMessage capturedRequest = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, token) => capturedRequest = request)
            .ReturnsAsync(httpResponseMessage);

        _divisasService.ObtenerUltimasTasas("EUR", "USD", null);

        Assert.That(capturedRequest.RequestUri.ToString(), Is.EqualTo(expectedUri));
    }
}