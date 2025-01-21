using System.Numerics;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Storage.Pdf.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banco_VivesBank.Storage.Pdf.Controller;

[ApiController]
[Route("api/[controller]")]
public class PdfTestController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly PdfStorage _pdfStorage;
    
    class DomiciliacionConcreta : Domiciliacion { }
    class IngresoNominaConcreta : IngresoNomina { }
    class PagoConTarjetaConcreto : PagoConTarjeta { }
    class TransferenciaConcreta : Transferencia { }
    
    public PdfTestController(PdfStorage pdfStorage)
    {
        _pdfStorage = pdfStorage;
    }

    [HttpGet("export-pdf")]
    public IActionResult ExportTestPdf()
    {
        // Crear datos de prueba para la cuenta
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
                Cliente = cuenta.Cliente,
                CreatedAt = DateTime.UtcNow,
                Domiciliacion = new DomiciliacionConcreta
                {
                    Cliente = cuenta.Cliente,
                    IbanOrigen = cuenta.Iban,
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
                Cliente = cuenta.Cliente,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IngresoNomina = new IngresoNominaConcreta
                {
                    NombreEmpresa = "hola",
                    CifEmpresa = "123456",
                    Importe = 1000,
                    IbanDestino = "4567899876456789876567",
                    IbanOrigen = cuenta.Iban
                }
            },
            new Movimiento
            {
                Cliente = cuenta.Cliente,
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
                Cliente = cuenta.Cliente,
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


        // Llamar al método ExportPDF con los datos de prueba
        _pdfStorage.ExportPDF(cuenta, movimientos);

        return Ok("PDF generado con éxito. Revise el archivo guardado.");
    }
}