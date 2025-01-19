using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<List<Models.Tarjeta>> GetAllAsync();
    public Task<TarjetaResponseDto> GetByGuidAsync(string id);
    public Task<TarjetaResponseDto> CreateAsync(TarjetaRequestDto dto);
    public Task<TarjetaResponseDto> UpdateAsync(string id, TarjetaRequestDto dto);
    public Task<TarjetaResponseDto> DeleteAsync(string id);
    
    public Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid);
    public Task<Models.Tarjeta?> GetTarjetaModelById(long id);
}