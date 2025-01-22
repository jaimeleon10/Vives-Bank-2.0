using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Services;

public interface IMovimientoService
{
    public Task<IEnumerable<MovimientoResponse>> GetAllAsync();
    public Task<MovimientoResponse?> GetByGuidAsync(string guid);
    public Task<IEnumerable<MovimientoResponse?>> GetByClienteGuidAsync(string clienteGuid);
    public Task<MovimientoResponse> CreateAsync(MovimientoRequest movimientoRequest);
    public Task<MovimientoResponse> CreateDomiciliacionAsync(DomiciliacionRequest domiciliacionRequest);
    public Task<MovimientoResponse> CreateIngresoNominaAsync(IngresoNominaRequest ingresoNomina);
    public Task<MovimientoResponse> CreatePagoConTarjetaAsync(PagoConTarjetaRequest pagoConTarjeta);
    public Task<MovimientoResponse> CreateTransferenciaAsync(TransferenciaRequest transferencia);
    public Task<MovimientoResponse> RevocarTransferencia(String movimientoGuid);
    public Task<MovimientoResponse?> UpdateAsync(string id, MovimientoRequest movimientoRequest);
    public Task<MovimientoResponse?> DeleteAsync(string id);
}