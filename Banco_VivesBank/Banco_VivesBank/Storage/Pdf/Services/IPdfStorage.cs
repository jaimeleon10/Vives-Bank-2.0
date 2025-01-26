using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Movimientos.Dto;

namespace Banco_VivesBank.Storage.Pdf.Services;

public interface IPdfStorage
{
    void ExportPDF(ClienteResponse cliente, List<MovimientoResponse> movimientos);
}