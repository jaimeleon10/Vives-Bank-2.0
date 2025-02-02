using System.Text;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Storage.Pdf.Exception;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Path = System.IO.Path;

namespace Banco_VivesBank.Storage.Pdf.Services;

public class PdfStorage : IPdfStorage
{
    private readonly ILogger<PdfStorage> _logger;

    public PdfStorage(ILogger<PdfStorage> logger)
    {
        _logger = logger;
    }
    
    public void ExportPDF(ClienteResponse cliente, List<MovimientoResponse> movimientos)
    {
        if (cliente == null)
            throw new ClienteNotFoundException("La cliente proporcionada es nula.");
        if (movimientos == null)
            throw new MovimientosInvalidosException("La lista de movimientos proporcionada es nula.");
    
        string html = movimientos.Count == 0
            ? $"<html><body><h1>No hay movimientos registrados para la cliente de {cliente.Nombre} {cliente.Apellidos}</h1></body></html>"
            : GenerarHtml(cliente, movimientos);

        var pdfDoc = GenerarPdf(html);

        if (pdfDoc.PageCount == 0)
            throw new PdfGenerateException("No se generaron páginas para el documento PDF.");

        string fechaHora = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", $"ReporteTransacciones_{cliente.Dni}_{fechaHora}.pdf");
        
        pdfDoc.Save(filePath);
        
        _logger.LogInformation("PDF generado y guardado en la carpeta data: {FilePath}", filePath);
    }

    private string GenerarHtml(ClienteResponse cliente, List<MovimientoResponse> movimientos)
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
                text-align: left;
            }}

            .iban {{
                font-size: 8px;
                font-weight: bold;
                text-align: center;
                max-width: 200px;
                overflow: hidden;
                white-space: nowrap;
                text-overflow: ellipsis;
            }}

            .container {{
                width: 100%;
                margin: 0 auto;
            }}

            .info-table {{
                width: 100%;
                margin-bottom: 20px;
            }}

            .info-table td {{
                padding: 5px;
            }}
        </style>
        <title>Reporte de Transacciones</title>
    </head>
    <body>
        <div class=""container"">
            <h1>Reporte de Transacciones de {cliente.Nombre} {cliente.Apellidos}</h1>
            
            <!-- Información combinada del cliente y banco en una tabla sin bordes -->
            <table class=""info-table"">
                <tr>
                    <td>
                        <h3>Datos de la Cuenta</h3>
                        <strong>Username:</strong> {cliente.UserResponse.Username}<br>
                        <strong>Nombre:</strong> {cliente.Nombre}<br>
                        <strong>Apellido:</strong> {cliente.Apellidos}<br>
                        <strong>DNI:</strong> {cliente.Dni}<br>
                        <strong>Email:</strong> {cliente.Email}<br>
                        <strong>Teléfono:</strong> {cliente.Telefono}<br>
                    </td>
                    <td style=""text-align: right;"">
                        <h3>Datos del Banco</h3>
                        <strong>Banco:</strong> VivesBank <br>
                        <strong>Fecha de Solicitud:</strong> {DateTime.Now:yyyy-MM-dd}<br>
                        <strong>Hora de Solicitud:</strong> {DateTime.Now:HH:mm:ss}<br>
                    </td>
                </tr>
            </table>
            
            <h3>Detalles de Movimientos</h3>
            {GenerarTablaMovimientos(movimientos)}
        </div>
    </body>
    </html>";
    }

    private string GenerarTablaMovimientos(List<MovimientoResponse> movimientos)
    {
        var tablaHtml = new StringBuilder();
        tablaHtml.AppendLine(@"
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
            var (tipoMovimiento, origen, destino, importe) = GetDetallesMovimientos(movimiento);
            tablaHtml.AppendLine($@"
        <tr>
            <td>{movimiento.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>
            <td>{tipoMovimiento}</td>
            <td class='iban'>{origen}</td>
            <td class='iban'>{destino}</td>
            <td>{importe}</td>
        </tr>");
        }

        tablaHtml.AppendLine("</tbody>\n</table>");
        return tablaHtml.ToString();
    }

    private (string tipo, string origen, string destino, string importe) GetDetallesMovimientos(MovimientoResponse movimiento)
    {
        return movimiento switch
        {
            { Domiciliacion: not null } => (
                "Domiciliación",
                movimiento.Domiciliacion.IbanEmpresa,
                movimiento.Domiciliacion.IbanCliente,
                movimiento.Domiciliacion.Importe.ToString()
            ),
            { IngresoNomina: not null } => (
                "Ingreso Nómina",
                movimiento.IngresoNomina.IbanEmpresa,
                movimiento.IngresoNomina.IbanCliente,
                movimiento.IngresoNomina.Importe.ToString()
            ),
            { PagoConTarjeta: not null } => (
                "Pago con Tarjeta",
                movimiento.PagoConTarjeta.NumeroTarjeta, 
                movimiento.PagoConTarjeta.NombreComercio, 
                movimiento.PagoConTarjeta.Importe.ToString()
            ),
            { Transferencia: not null } => (
                "Transferencia",
                movimiento.Transferencia.IbanOrigen,
                movimiento.Transferencia.IbanDestino,
                movimiento.Transferencia.Importe.ToString()
            ),
            _ => ("Desconocido", "", "", "")
        };
    }

    public  PdfDocument GenerarPdf(string html)
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
        return pdf;
    }
}
