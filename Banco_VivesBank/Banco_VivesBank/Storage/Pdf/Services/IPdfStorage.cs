using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Movimientos.Dto;

namespace Banco_VivesBank.Storage.Pdf.Services;

/// <summary>
/// Interfaz que define los métodos para el almacenamiento y generación de documentos PDF
/// con los detalles de las transacciones de un cliente.
/// </summary>
public interface IPdfStorage
{
    /// <summary>
    /// Exporta un archivo PDF que contiene el reporte de transacciones de un cliente.
    /// </summary>
    /// <param name="cliente">Objeto que contiene la información del cliente.</param>
    /// <param name="movimientos">Lista de movimientos asociados al cliente.</param>
    void ExportPDF(ClienteResponse cliente, List<MovimientoResponse> movimientos);
}