using System.Text;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Banco_VivesBank.Storage.Pdf.Services;

public class PdfStorage : IPdfStorage
{
    private readonly ILogger<PdfStorage> _logger;

    public PdfStorage(ILogger<PdfStorage> logger)
    {
        _logger = logger;
    }

    public void ExportPDF(Cuenta cuenta, List<Movimiento> movimientos)
    {
        var html = GenerateHtml(cuenta, movimientos);
        var pdfDoc = CreatePdfDocument(html);
        string fechaHora = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        pdfDoc.Save(Path.Combine(Directory.GetCurrentDirectory(), "data", $"ReporteTransacciones_{cuenta.Cliente.Dni}_{fechaHora}.pdf"));

        _logger.LogInformation("PDF generado y guardado en la carpeta data.");
    }

    private string GenerateHtml(Cuenta cuenta, List<Movimiento> movimientos)
    {
return $@"
<html>
<head>
    <style>
        /* General page styles */
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 20px;
        }}

        h1 {{
            text-align: center;
            color: #333;
            font-size: 28px;
            margin-bottom: 20px;
        }}

        h3 {{
            color: #555;
            font-size: 20px;
            margin-top: 20px;
        }}

        table {{
            width: 100%;
table-layout: fixed;
            border-collapse: collapse;
            margin-top: 20px;
        }}

        th, td {{
            padding: 12px 15px;
            text-align: center;
            border: 1px solid #ddd;
            word-wrap: break-word; 
        }}
    </style>
    <title>Reporte de Transacciones</title>
</head>
<body>
    <div class='container'>
        <h1>Reporte de Transacciones</h1>
        {GenerateClienteInfo(cuenta)}
        <h3>Detalles de Movimientos</h3>
        {GenerateMovimientosTable(movimientos)}
    </div>
</body>
</html>";
    }

    private string GenerateClienteInfo(Cuenta cuenta)
    {
        return $@"
        <h3>Datos de la Cuenta</h3>
        <strong>Nombre:</strong> {cuenta.Cliente.Nombre}<br>
        <strong>Apellido:</strong> {cuenta.Cliente.Apellidos}<br>
        <strong>DNI:</strong> {cuenta.Cliente.Dni}<br>
        <strong>Email:</strong> {cuenta.Cliente.Email}<br>
        <strong>Teléfono:</strong> {cuenta.Cliente.Telefono}<br>
        <strong>IBAN de la Cuenta:</strong> {cuenta.Iban}</p>
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
                    <th>Origen</th>
                    <th>Destino</th>
                    <th>Importe</th>
                </tr>
            </thead>
            <tbody>");

        foreach (var movimiento in movimientos)
        {
            var (tipoMovimiento, origen, destino, importe) = GetMovimientoDetails(movimiento);
            htmlTable.AppendLine($@"
        <tr>
            <td>{movimiento.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>
            <td>{tipoMovimiento}</td>
            <td>{origen}</td>
            <td>{destino}</td>
            <td>{importe}</td>
        </tr>");
        }

        htmlTable.AppendLine("</tbody>\n</table>");
        return htmlTable.ToString();
    }

    private (string tipo, string origen, string destino, string importe) GetMovimientoDetails(Movimiento movimiento)
    {
        return movimiento switch
        {
            { Domiciliacion: not null } => (
                "Domiciliación",
                movimiento.Domiciliacion.IbanOrigen,
                movimiento.Domiciliacion.IbanDestino,
                movimiento.Domiciliacion.Importe.ToString("F2")
            ),
            { IngresoNomina: not null } => (
                "Ingreso Nómina",
                movimiento.IngresoNomina.IbanOrigen,
                movimiento.IngresoNomina.IbanDestino,
                movimiento.IngresoNomina.Importe.ToString("F2")
            ),
            { PagoConTarjeta: not null } => (
                "Pago con Tarjeta",
                movimiento.PagoConTarjeta.NumeroTarjeta, 
                "Aprobado", 
                movimiento.PagoConTarjeta.Importe.ToString("F2") 
            ),
            { Transferencia: not null } => (
                "Transferencia",
                movimiento.Transferencia.IbanOrigen,
                movimiento.Transferencia.IbanDestino,
                movimiento.Transferencia.Importe.ToString("F2") 
            ),
            _ => ("Desconocido", "", "", "")
        };
    }

    private PdfDocument CreatePdfDocument(string html)
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
        return pdf;
    }
}
