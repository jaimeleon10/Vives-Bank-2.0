using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<List<TarjetaResponse>> GetAllAsync();
    public Task<TarjetaResponse?> GetByGuidAsync(string id);
    public Task<TarjetaResponse?> GetByNumeroTarjetaAsync(string numeroTarjeta);
    public Task<TarjetaResponse> CreateAsync(TarjetaRequest dto);
    public Task<TarjetaResponse?> UpdateAsync(string id, TarjetaRequest dto);
    public Task<TarjetaResponse?> DeleteAsync(string id);
    
    public Task<Models.TarjetaModel?> GetTarjetaModelByGuidAsync(string guid);
    public Task<Models.TarjetaModel?> GetTarjetaModelByIdAsync(long id);
}