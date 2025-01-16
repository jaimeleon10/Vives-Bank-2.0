using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Vives_Bank_Net.Frankfurter.Services;
using NUnit.Framework;
using Vives_Bank_Net.Frankfurter.Exceptions;

[TestFixture]
public class DivisasServiceTest
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private DivisasService _divisasService;

    public DivisasServiceTest()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _divisasService = new DivisasService(_httpClient);
    }

    [Test]
    public async Task ObtenerCambioDeDivisas()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13,\"GBP\":0.88},\"amount\":1}";
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
        Assert.That(result.Rates["USD"], Is.EqualTo(1.13m));
    }

    [Test]
    public async Task ObtenerCambioSinParametros()
    {
        var jsonResponse = "{\"rates\":{\"USD\":1.13,\"GBP\":0.88},\"amount\":1}";
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

        var result = _divisasService.ObtenerUltimasTasas("", "", "");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Rates["USD"], Is.EqualTo(1.13));
        Assert.That(result.Rates["GBP"], Is.EqualTo(0.88));
    }
    
}
