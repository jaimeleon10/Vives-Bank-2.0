using Moq;
using NUnit.Framework;
using Vives_Bank_Net.Frankfurter.Controller;
using Vives_Bank_Net.Frankfurter.Services;


[TestFixture]
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
    public void ObtenerDivisasTest()
    {
        //
    }
}
