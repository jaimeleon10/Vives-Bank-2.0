using System.Text;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace Banco_VivesBank.Storage.Pdf.Services;

public class PdfStorage : IPdfStorage
{
    private readonly ILogger<PdfStorage> _logger;
    private readonly IConverter _converter;
    private const string OUTPUT_FILENAME = "ReporteTransacciones.pdf";

    public PdfStorage(ILogger<PdfStorage> logger, IConverter converter)
    {
        _logger = logger;
        _converter = converter;
    }

    public void ExportPDF(Cuenta cuenta, List<Movimiento> movimientos)
    {
        var html = GenerateHtml(cuenta, movimientos);
        var pdfDoc = CreatePdfDocument(html);
        
        _converter.Convert(pdfDoc);
        _logger.LogInformation("PDF generado y guardado en la carpeta data.");
    }

    private string GenerateHtml(Cuenta cuenta, List<Movimiento> movimientos)
    {
        return $@"
<html>
<head>
    <style>
        table {{ 
            width: 100%;
            border-collapse: collapse;
        }}
        th, td {{
            border: 1px solid black;
            padding: 8px;
            text-align: left;
        }}
        th {{
            background-color: #f2f2f2;
        }}
    </style>
    <title>Reporte de Transacciones</title>
</head>
<body>
    <h1>Reporte de Transacciones</h1>
    {GenerateClienteInfo(cuenta)}
    <h3>Detalles de Movimientos</h3>
    {GenerateMovimientosTable(movimientos)}
</body>
</html>";
    }

    private string GenerateClienteInfo(Cuenta cuenta)
    {
        return $@"
    <h3>Datos de la Cuenta</h3>
    <p><strong>Nombre:</strong> {cuenta.Cliente.Nombre}</p>
    <p><strong>Apellido:</strong> {cuenta.Cliente.Apellidos}</p>
    <p><strong>DNI:</strong> {cuenta.Cliente.Dni}</p>
    <p><strong>Email:</strong> {cuenta.Cliente.Email}</p>
    <p><strong>Teléfono:</strong> {cuenta.Cliente.Telefono}</p>
    <p><strong>IBAN de la Cuenta:</strong> {cuenta.Iban}</p>
    <br>";
    }

    private string GenerateMovimientosTable(List<Movimiento> movimientos)
    {
        var htmlTable = new StringBuilder();
        htmlTable.AppendLine(@"
<table border='1'>
    <thead>
        <tr>
            <th>Fecha</th>
            <th>Tipo de Movimiento</th>
            <th>Detalle</th>
        </tr>
    </thead>
    <tbody>");

        foreach (var movimiento in movimientos)
        {
            var (tipoMovimiento, detalle) = GetMovimientoDetails(movimiento);
            htmlTable.AppendLine($@"
        <tr>
            <td>{movimiento.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>
            <td>{tipoMovimiento}</td>
            <td>{detalle}</td>
        </tr>");
        }

        htmlTable.AppendLine("    </tbody>\n</table>");
        return htmlTable.ToString();
    }

    private (string tipo, string detalle) GetMovimientoDetails(Movimiento movimiento)
    {
        return movimiento switch
        {
            { Domiciliacion: not null } => (
                "Domiciliación",
                $"Acreedor: {movimiento.Domiciliacion.Acreedor}, Importe: {movimiento.Domiciliacion.Importe}"
            ),
            { IngresoNomina: not null } => (
                "Ingreso Nómina",
                $"CIF: {movimiento.IngresoNomina.CifEmpresa}, Importe: {movimiento.IngresoNomina.Importe}"
            ),
            { PagoConTarjeta: not null } => (
                "Pago con Tarjeta",
                $"Tarjeta: {movimiento.PagoConTarjeta.NumeroTarjeta}, Comercio: {movimiento.PagoConTarjeta.NombreComercio}, Importe: {movimiento.PagoConTarjeta.Importe}"
            ),
            { Transferencia: not null } => (
                "Transferencia",
                $"Destino: {movimiento.Transferencia.IbanDestino}, Importe: {movimiento.Transferencia.Importe}"
            ),
            _ => ("Desconocido", "")
        };
    }

    private HtmlToPdfDocument CreatePdfDocument(string html)
    {
        // Crear el documento PDF
        var pdfDocument = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Out = Path.Combine(Directory.GetCurrentDirectory(), "data", OUTPUT_FILENAME)
            }
        };

        var objectSettings = new ObjectSettings
        {
            HtmlContent = html,
            WebSettings = new WebSettings
            {
                DefaultEncoding = "utf-8"
            }
        };

        pdfDocument.Objects.Add(objectSettings);

        return pdfDocument;
    }
}