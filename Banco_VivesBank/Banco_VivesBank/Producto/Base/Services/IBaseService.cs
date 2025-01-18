using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Services;

public interface IBaseService
{
    public Task<IEnumerable<BaseModel>> GetAllAsync();
    public Task<BaseResponseDto> GetByGuidAsync(string id);
    public Task<BaseResponseDto> GetByTipoAsync(string tipo);
    public Task<BaseResponseDto> CreateAsync(BaseRequestDto baseRequest);
    public Task<BaseResponseDto> UpdateAsync(string id, BaseUpdateDto baseUpdate);
    public Task<BaseResponseDto> DeleteAsync(string id);
}