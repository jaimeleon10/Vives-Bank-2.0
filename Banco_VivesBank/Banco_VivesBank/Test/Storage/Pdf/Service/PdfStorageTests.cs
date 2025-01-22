using System.Numerics;
using Moq;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Storage.Pdf.Exceptions;
using Banco_VivesBank.Storage.Pdf.Services;
using NUnit.Framework;
using System.Text;
using PdfSharp.Pdf;

namespace Banco_VivesBank.Test.Storage;

[TestFixture]
public class PdfStorageTests
{
    class DomiciliacionConcreta : Domiciliacion { }
    class IngresoNominaConcreta : IngresoNomina { }
    class PagoConTarjetaConcreto : PagoConTarjeta { }
    class TransferenciaConcreta : Transferencia { }
    
    private Mock<ILogger<PdfStorage>> _loggerMock;
    private PdfStorage _pdfStorage;
    private string _dataPath;
    private Cuenta _cuenta;

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PdfStorage>>();
        _pdfStorage = new PdfStorage(_loggerMock.Object);
        _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        _cuenta = CreateSampleCuenta();

        if (Directory.Exists(_dataPath))
        {
            var files = Directory.GetFiles(_dataPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(_dataPath);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cuenta.Cliente.Dni}_*.pdf");
        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    [Test]
    public void ExportPDF()
    {
        var movimientos = CreateSampleMovimientos();

        Assert.DoesNotThrow(() => _pdfStorage.ExportPDF(_cuenta, movimientos));
        
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cuenta.Cliente.Dni}_*.pdf");
        Assert.That(files.Length, Is.EqualTo(1), "El archivo PDF debería haberse generado.");
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("PDF generado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Test]
    public void ExportPDFCuentaNull()
    {
        // Arrange
        var movimientos = CreateSampleMovimientos();

        // Act & Assert
        var ex = Assert.Throws<CuentaInvalidaException>(() => 
            _pdfStorage.ExportPDF(null, movimientos));
        Assert.That(ex.Message, Is.Not.Empty);
    }
    
    [Test]
    public void ExportPDFNullMovimientos()
    {
        // Act & Assert
        var ex = Assert.Throws<MovimientosInvalidosException>(() => 
            _pdfStorage.ExportPDF(_cuenta, null));
        Assert.That(ex.Message, Is.Not.Empty);
    }

    [Test]
    public void ExportPDFEmptyMovimientos()
    {
        // Arrange
        var movimientos = new List<Movimiento>();

        // Act
        Assert.DoesNotThrow(() => _pdfStorage.ExportPDF(_cuenta, movimientos));

        // Assert
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cuenta.Cliente.Dni}_*.pdf");
        Assert.That(files.Length, Is.EqualTo(1), "El archivo PDF debería generarse incluso sin movimientos.");

        // Limpieza adicional por si es necesario
        if (files.Length > 0)
        {
            File.Delete(files[0]);
        }
    }
    
    private Cuenta CreateSampleCuenta()
    {
        return new Cuenta
        {
            Cliente = new Banco_VivesBank.Cliente.Models.Cliente
            {
                Nombre = "John",
                Apellidos = "Doe",
                Dni = "12345678A",
                Email = "john.doe@example.com",
                Telefono = "123456789"
            },
            Iban = "ES1234567890123456789012"
        };
    }

    private List<Movimiento> CreateSampleMovimientos()
    {
        return new List<Movimiento>
        {
            new Movimiento
            {
                Cliente = _cuenta.Cliente,
                CreatedAt = DateTime.UtcNow,
                Domiciliacion = new DomiciliacionConcreta
                {
                    Cliente = _cuenta.Cliente,
                    IbanOrigen = _cuenta.Iban,
                    IbanDestino = "ES00998877665544332211",
                    Importe = new BigInteger(50000),
                    Acreedor = "Compañía Eléctrica",
                    FechaInicio = new DateTime(2024, 1, 1),
                    Periodicidad = Periodicidad.Mensual,
                    Activa = true,
                    UltimaEjecucion = DateTime.UtcNow.AddDays(-30)
                }
            },
            new Movimiento
            {
                Cliente = _cuenta.Cliente,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IngresoNomina = new IngresoNominaConcreta
                {
                    NombreEmpresa = "Empresa ABC",
                    CifEmpresa = "B12345678",
                    Importe = new BigInteger(200000),
                    IbanDestino = _cuenta.Iban,
                    IbanOrigen = "ES11223344556677889900"
                }
            },
            new Movimiento
            {
                Cliente = _cuenta.Cliente,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                PagoConTarjeta = new PagoConTarjetaConcreto
                {
                    NumeroTarjeta = "4111111111111111",
                    NombreComercio = "Tienda XYZ",
                    Importe = new BigInteger(50000)
                }
            },
            new Movimiento
            {
                Cliente = _cuenta.Cliente,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Transferencia = new TransferenciaConcreta
                {
                    IbanOrigen = _cuenta.Iban,
                    NombreBeneficiario = "Jane Doe",
                    IbanDestino = "ES12345678901234567890",
                    Importe = new BigInteger(50000)
                }
            }
        };
    }
}