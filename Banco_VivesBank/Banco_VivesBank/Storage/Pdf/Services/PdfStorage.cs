using Banco_VivesBank.Storage.Pdf.Exception;
using Banco_VivesBank.Cliente.Exceptions;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Cliente.Dto;
using Path = System.IO.Path;
using PdfSharp.Pdf;
using System.Text;

namespace Banco_VivesBank.Storage.Pdf.Services;

/// <summary>
/// Clase que proporciona servicios relacionados con la generación y almacenamiento de archivos PDF
/// con detalles sobre transacciones y clientes.
/// </summary>
public class PdfStorage : IPdfStorage
{
    private readonly ILogger<PdfStorage> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PdfStorage"/>.
    /// </summary>
    /// <param name="logger">Instancia del registrador para los logs.</param>
    public PdfStorage(ILogger<PdfStorage> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Exporta un archivo PDF con el reporte de transacciones de un cliente.
    /// </summary>
    /// <param name="cliente">Objeto con la información del cliente.</param>
    /// <param name="movimientos">Lista de movimientos asociados al cliente.</param>
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

    /// <summary>
    /// Genera el contenido HTML para el reporte de transacciones de un cliente.
    /// </summary>
    /// <param name="cliente">Objeto con la información del cliente.</param>
    /// <param name="movimientos">Lista de movimientos a incluir en el reporte.</param>
    /// <returns>El contenido HTML para el reporte.</returns>
    private string GenerarHtml(ClienteResponse cliente, List<MovimientoResponse> movimientos)
    {
        return $@"
    <html>
    <head>
        <style>
        @import url('https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;700&display=swap');

        body {{
            font-family: 'Roboto', sans-serif;
            margin: 0;
            margin-top: -40px;
            padding: 20px;
            background-color: #f4f4f4;
            color: #333;
        }}

        .container {{
            max-width: 800px;
            background: #fff;
            padding: 20px;
            margin: 10px auto;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            border-radius: 10px;
        }}

        h1 {{
            text-align: center;
            color: #2c3e50;
            font-size: 26px;
            margin-top: 10px; /* Reducimos el margen superior */
            font-weight: 700;
        }}

        h3 {{
            color: #34495e;
            font-size: 18px;
            margin-top: 15px;
            border-bottom: 2px solid #ddd;
            padding-bottom: 5px;
        }}

        /* Alineación de Datos de la Cuenta y del Banco */
        .info-table {{
            width: 100%;
            margin-bottom: 15px;
            border-collapse: collapse;
        }}

        .info-table td {{
            padding: 8px;
            vertical-align: top; /* Alinea los datos al inicio */
        }}

        .info-table tr td:first-child {{
            width: 50%;
        }}

        .tabla_movim {{
            width: 100%;
            border-collapse: collapse;
            font-family: Arial, sans-serif;
        }}

        .tabla_movim th, 
        .tabla_movim td {{
            border: 1px solid #ddd;
            padding: 10px;
            text-align: center;
        }}

        .tabla_movim thead {{
            background-color: #2c3e50;
            color: black;
            font-weight: bold;
        }}

        /* Ajustamos el ancho de las columnas de Origen y Destino */
        .col-origen, .col-destino {{
            width: 30%;
            word-wrap: break-word;
        }}

        .footer {{
            text-align: center;
            font-size: 12px;
            margin-top: 20px;
            color: #777;
        }}
        .iban {{
         font-size: 8px;
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

        <div class=""footer"">
            VivesBank - Todos los derechos reservados 2021
        </div>
        </body>
    </html>";
    }

    /// <summary>
    /// Genera el contenido HTML para la tabla de movimientos.
    /// </summary>
    /// <param name="movimientos">Lista de movimientos para generar la tabla.</param>
    /// <returns>El contenido HTML de la tabla de movimientos.</returns>
    private string GenerarTablaMovimientos(List<MovimientoResponse> movimientos)
    {
        var tablaHtml = new StringBuilder();
        tablaHtml.AppendLine(@"
        <table class=""tabla_movim"", border='1'>
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

        tablaHtml.AppendLine("</tbody>\n</table>\n");
        return tablaHtml.ToString();
    }

    /// <summary>
    /// Obtiene los detalles de un movimiento específico para generar el reporte.
    /// </summary>
    /// <param name="movimiento">El movimiento a procesar.</param>
    /// <returns>Detalles del movimiento como tipo, origen, destino e importe.</returns>
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

    /// <summary>
    /// Genera un documento PDF a partir del contenido HTML proporcionado.
    /// </summary>
    /// <param name="html">Contenido HTML a convertir en PDF.</param>
    /// <returns>El documento PDF generado.</returns>
    public  PdfDocument GenerarPdf(string html)
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
        return pdf;
    }
}
