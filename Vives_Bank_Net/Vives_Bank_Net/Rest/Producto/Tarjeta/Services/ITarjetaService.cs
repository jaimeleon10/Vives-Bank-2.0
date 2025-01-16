using Vives_Bank_Net.Rest.Producto.Tarjeta.Dto;
using Vives_Bank_Net.Rest.Producto.Tarjeta.Models;

namespace Vives_Bank_Net.Rest.Producto.Tarjeta.Services;

public interface ITarjetaService
{
    public Task<List<TarjetaModel>> GetAllAsync();
    public Task<TarjetaResponseDto> GetByGuidAsync(string id);
    public Task<TarjetaResponseDto> CreateAsync(TarjetaRequestDto dto);
    public Task<TarjetaResponseDto> UpdateAsync(string id, TarjetaRequestDto dto);
    public Task<TarjetaResponseDto> DeleteAsync(string id);
}