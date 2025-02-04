using Banco_VivesBank.Movimientos.Dto;

namespace Banco_VivesBank.Movimientos.Services.Movimientos;

public interface IMovimientoService
{
    public Task<IEnumerable<MovimientoResponse>> GetAllAsync();
    public Task<MovimientoResponse?> GetByGuidAsync(string guid);
    public Task<IEnumerable<MovimientoResponse>> GetByClienteGuidAsync(string clienteGuid);
    public Task<IEnumerable<MovimientoResponse>> GetMyMovimientos(User.Models.User userAuth);
    
    public Task CreateAsync(MovimientoRequest movimientoRequest);
    public Task<IngresoNominaResponse> CreateIngresoNominaAsync(User.Models.User userAuth, IngresoNominaRequest ingresoNomina);
    public Task<PagoConTarjetaResponse> CreatePagoConTarjetaAsync(User.Models.User userAuth, PagoConTarjetaRequest pagoConTarjeta);
    public Task<TransferenciaResponse> CreateTransferenciaAsync(User.Models.User userAuth, TransferenciaRequest transferencia);
    
    public Task<TransferenciaResponse> RevocarTransferenciaAsync(User.Models.User userAuth, String movimientoGuid);
}