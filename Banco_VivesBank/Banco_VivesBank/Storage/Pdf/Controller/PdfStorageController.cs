using System.Numerics;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Storage.Pdf.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Storage.Pdf.Controller;

[ApiController]
[Route("api/[controller]")]
public class PdfStorageController : Microsoft.AspNetCore.Mvc.Controller
{
    class DomiciliacionConcreta : Domiciliacion { }
    class IngresoNominaConcreta : IngresoNomina { }
    class PagoConTarjetaConcreto : PagoConTarjeta { }
    class TransferenciaConcreta : Transferencia { }
    
    private readonly PdfStorage _pdfStorage;

    public PdfStorageController(IPdfStorage pdfStorage)
    {
        _pdfStorage = pdfStorage as PdfStorage ?? throw new ArgumentNullException(nameof(pdfStorage));
    }

    [HttpGet("export-pdf")]
    public IActionResult ExportTestPdf()
    {
        var cuenta = new Cuenta
        {
            Cliente = new Cliente.Models.Cliente
            {
                Nombre = "Juan",
                Apellidos = "Pérez",
                Dni = "12345678X",
                Email = "juan.perez@example.com",
                Telefono = "123456789"
            },
            Iban = "ES12345678901234567890"
        };
        
        var movimientos = new List<Movimiento>
        {
            new Movimiento
            {
                ClienteGuid = cuenta.Cliente.Guid,
                CreatedAt = DateTime.UtcNow,
                Domiciliacion = new DomiciliacionConcreta
                {
                    ClienteGuid = cuenta.Cliente.Guid,
                    IbanEmpresa = cuenta.Iban,
                    IbanCliente = "ES00998877665544332211",
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
                ClienteGuid = cuenta.Cliente.Guid,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IngresoNomina = new IngresoNominaConcreta
                {
                    NombreEmpresa = "hola",
                    CifEmpresa = "123456",
                    Importe = 1000,
                    IbanEmpresa = "4567899876456789876567",
                    IbanCliente = cuenta.Iban
                }
            },
            new Movimiento
            {
                ClienteGuid = cuenta.Cliente.Guid,
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
                ClienteGuid = cuenta.Cliente.Guid,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Transferencia = new TransferenciaConcreta
                {
                    IbanOrigen = cuenta.Iban,
                    NombreBeneficiario = "hola",
                    IbanDestino = "ES12345678901234567890",
                    Importe = new BigInteger(50000)
                }
            }
        };

        _pdfStorage.ExportPDF(cuenta, movimientos);
        
        return Ok("PDF generado");
    }
}