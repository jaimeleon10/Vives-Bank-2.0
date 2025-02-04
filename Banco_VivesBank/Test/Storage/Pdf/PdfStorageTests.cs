using System.Text;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Storage.Pdf.Exception;
using Banco_VivesBank.Storage.Pdf.Services;
using Banco_VivesBank.User.Dto;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Storage.Pdf;

[TestFixture]
public class PdfStorageTests
{
    
    private Mock<ILogger<PdfStorage>> _loggerMock;
    private PdfStorage _pdfStorage;
    private string _dataPath;
    private ClienteResponse _cliente;

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
        _cliente = CreateSampleCliente();

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
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cliente.Dni}_*.pdf");
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

        Assert.DoesNotThrow(() => _pdfStorage.ExportPDF(_cliente, movimientos));
        
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cliente.Dni}_*.pdf");
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
    public void ExportPDF_ClienteNull()
    {
        var movimientos = CreateSampleMovimientos();
    
        var ex = Assert.Throws<ClienteNotFoundException>(() => 
            _pdfStorage.ExportPDF(null, movimientos));
    
        Assert.That(ex.Message, Is.Not.Empty);
    }
    
    [Test]
    public void ExportPDFNullMovimientos()
    {
        // Act & Assert
        var ex = Assert.Throws<MovimientosInvalidosException>(() => 
            _pdfStorage.ExportPDF(_cliente, null));
        Assert.That(ex.Message, Is.Not.Empty);
    }

    [Test]
    public void ExportPDFEmptyMovimientos()
    {
        var movimientos = new List<MovimientoResponse>();

        Assert.DoesNotThrow(() => _pdfStorage.ExportPDF(_cliente, movimientos));
        
        var files = Directory.GetFiles(_dataPath, $"ReporteTransacciones_{_cliente.Dni}_*.pdf");
        Assert.That(files.Length, Is.EqualTo(1), "El archivo PDF debería generarse incluso sin movimientos.");

        if (files.Length > 0)
        {
            File.Delete(files[0]);
        }
    }
    
    private ClienteResponse CreateSampleCliente()
    {
        return new ClienteResponse
        {
            Guid = "LTtXSvg383G",
            Dni = "12345678A",
            Nombre = "Juan",
            Apellidos = "Pérez García",
            Direccion = new Direccion
            {
                Numero = "7",
                Calle = "Calle Ficticia 123",
                CodigoPostal = "28001",
                Letra = "a",
                Piso = "4"
            },
            Email = "juan.perez@example.com",
            Telefono = "+34 600 000 000",
            FotoPerfil = "url_foto_perfil.jpg",
            FotoDni = "url_foto_dni.jpg", 
            UserResponse = new UserResponse
            {
                Username = "juanperez", 
                Role = "User"
            },
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            IsDeleted = false 
        };
    }


    private List<MovimientoResponse> CreateSampleMovimientos()
    {
        return new List<MovimientoResponse>
        {
            new MovimientoResponse
            {
                Guid = "git_mov_1",
                ClienteGuid = _cliente.Guid,
                Domiciliacion = new DomiciliacionResponse
                {
                    Guid = "dom_1",
                    ClienteGuid = _cliente.Guid,
                    IbanEmpresa = "ES00998877665544387856",
                    IbanCliente = "ES00998877665544332211",
                    Importe = 50000,
                    Acreedor = "Compañía Eléctrica",
                    FechaInicio = new DateTime(2024, 1, 1).ToString(),
                    Periodicidad = "Mensual",
                    Activa = true
                },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            },
            new MovimientoResponse
            {
                Guid = "git_mov_2",
                ClienteGuid = _cliente.Guid,
                IngresoNomina = new IngresoNominaResponse
                {
                    NombreEmpresa = "Empresa ABC",
                    CifEmpresa = "B12345678",
                    Importe = 200000,
                    IbanCliente = "ES11223344556677889900",
                    IbanEmpresa = "ES11223344556677889900"
                },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            },
            new MovimientoResponse
            {
                Guid = "git_mov_3",
                ClienteGuid = _cliente.Guid,
                PagoConTarjeta = new PagoConTarjetaResponse
                {
                    NumeroTarjeta = "4111111111111111",
                    NombreComercio = "Tienda XYZ",
                    Importe = 50000
                },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            },
            new MovimientoResponse
            {
                Guid = "git_mov_4",
                ClienteGuid = _cliente.Guid,
                Transferencia = new TransferenciaResponse
                {
                    IbanOrigen = "ES12345678901234567456",
                    NombreBeneficiario = "Jane Doe",
                    IbanDestino = "ES12345678901234567890",
                    Importe = 50000
                },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };
    }
}