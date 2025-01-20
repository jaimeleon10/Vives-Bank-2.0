using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<List<Models.Tarjeta>> GetAllAsync();
    public Task<TarjetaResponse> GetByGuidAsync(string id);
    public Task<TarjetaResponse> CreateAsync(TarjetaRequest dto);
    public Task<TarjetaResponse> UpdateAsync(string id, TarjetaRequest dto);
    public Task<TarjetaResponse> DeleteAsync(string id);
    
    public Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid);
    public Task<Models.Tarjeta?> GetTarjetaModelById(long id);
}