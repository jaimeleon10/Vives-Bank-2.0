using Banco_VivesBank.Movimientos.Dto;

namespace Banco_VivesBank.Movimientos.Services.Movimientos;

public interface IMovimientoService
{
    public Task<IEnumerable<MovimientoResponse>> GetAllAsync();
    public Task<MovimientoResponse?> GetByGuidAsync(string guid);
    public Task<IEnumerable<MovimientoResponse>> GetByClienteGuidAsync(string clienteGuid);
    public Task CreateAsync(MovimientoRequest movimientoRequest);
    public Task<IngresoNominaResponse> CreateIngresoNominaAsync(IngresoNominaRequest ingresoNomina);
    public Task<PagoConTarjetaResponse> CreatePagoConTarjetaAsync(PagoConTarjetaRequest pagoConTarjeta);
    public Task<TransferenciaResponse> CreateTransferenciaAsync(TransferenciaRequest transferencia);
    public Task<TransferenciaResponse> RevocarTransferenciaAsync(String movimientoGuid);
}