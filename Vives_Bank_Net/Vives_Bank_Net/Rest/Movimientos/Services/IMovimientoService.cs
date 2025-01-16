using Vives_Bank_Net.Rest.Movimientos.Models;

namespace Vives_Bank_Net.Rest.Movimientos.Services;

public interface IMovimientoService
{
    public Task<IEnumerable<Movimiento>> GetAllAsync();
    public Task<Movimiento?> GetByIdAsync(string id);
    public Task<Movimiento> CreateAsync(Movimiento movimiento);
    public Task<Movimiento> CreateDomiciliacionAsync(Domiciliacion domiciliacion);
    public Task<Movimiento> CreateIngresoNominaAsync(IngresoNomina ingresoNomina);
    public Task<Movimiento> CreatePagoConTarjetaAsync(PagoConTarjeta pagoConTarjeta);
    public Task<Movimiento> CreateTransferenciaAsync(Transferencia transferencia);
    public Task<Movimiento> RevocarTransferencia(String movimientoGuid);
    public Task<Movimiento?> UpdateAsync(string id, Movimiento movimiento);
    public Task<Movimiento?> DeleteAsync(string id);
}