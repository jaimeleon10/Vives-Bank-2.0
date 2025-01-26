using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<PageResponse<TarjetaResponse>> GetAllPagedAsync(PageRequest pageRequest);
    public Task<TarjetaResponse?> GetByGuidAsync(string guid);
    public Task<TarjetaResponse?> GetByNumeroTarjetaAsync(string numeroTarjeta);
    public Task<TarjetaResponse> CreateAsync(TarjetaRequest dto);
    public Task<TarjetaResponse?> UpdateAsync(string guid, TarjetaRequestUpdate dto);
    public Task<TarjetaResponse?> DeleteAsync(string guid);
    
    public Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid);
    public Task<Models.Tarjeta?> GetTarjetaModelById(long id);
    
    public Task<List<Models.Tarjeta>> GetAllForStorage();
}