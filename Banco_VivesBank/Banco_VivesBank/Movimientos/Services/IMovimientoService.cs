using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Services;

public interface IMovimientoService
{
    public Task<IEnumerable<Movimiento>> GetAllAsync();
    public Task<Movimiento?> GetByIdAsync(string id);
    public Task<Movimiento?> GetByGuidAsync(string guid);
    public Task<Movimiento?> GetByClinteGuidAsync(string clienteGuid);
    public Task<Domiciliacion?> GetDomiciliacionByGuidAsync(string domiciliacionGuid);
    public Task<Domiciliacion?> GetDomiciliacionByClienteGuidAsync(string clienteGuid);
    public Task<Movimiento> CreateAsync(MovimientoRequest movimientoRequest);
    public Task<Movimiento> CreateDomiciliacionAsync(Domiciliacion domiciliacion);
    public Task<Movimiento> CreateIngresoNominaAsync(IngresoNomina ingresoNomina);
    public Task<Movimiento> CreatePagoConTarjetaAsync(PagoConTarjeta pagoConTarjeta);
    public Task<Movimiento> CreateTransferenciaAsync(Transferencia transferencia);
    public Task<Movimiento> RevocarTransferencia(String movimientoGuid);
    public Task<Movimiento?> UpdateAsync(string id, Movimiento movimiento);
    public Task<Movimiento?> DeleteAsync(string id);
}