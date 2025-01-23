using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<List<TarjetaResponse>> GetAllAsync();
    public Task<TarjetaResponse> GetByGuidAsync(string id);
    public Task<TarjetaResponse> CreateAsync(TarjetaRequest dto);
    public Task<TarjetaResponse> UpdateAsync(string id, TarjetaRequest dto);
    public Task<TarjetaResponse> DeleteAsync(string id);
    public Task<PageResponse<TarjetaResponse>> GetAllPagedAsync(PageRequest pageRequest);
    
    public Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid);
    public Task<Models.Tarjeta?> GetTarjetaModelById(long id);
}