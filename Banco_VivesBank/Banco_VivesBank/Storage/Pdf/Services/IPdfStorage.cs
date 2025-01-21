using Banco_VivesBank.Movimientos.Models;
using Banco_VivesBank.Producto.Cuenta.Models;

namespace Banco_VivesBank.Storage.Pdf.Services;

public interface IPdfStorage
{
    void ExportPDF(Cuenta cuenta, List<Movimiento> movimientos);
}