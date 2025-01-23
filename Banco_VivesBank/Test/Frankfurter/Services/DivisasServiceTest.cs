using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Banco_VivesBank.Frankfurter.Exceptions;
using Banco_VivesBank.Frankfurter.Services;

namespace Test;

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
}