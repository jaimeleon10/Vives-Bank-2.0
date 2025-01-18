using Banco_VivesBank.Frankfurter.Controller;
using Banco_VivesBank.Frankfurter.Services;
using Moq;
using NUnit.Framework;

namespace Banco_VivesBank.Test.Frankfurter.Controller;

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