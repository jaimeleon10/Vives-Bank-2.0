using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Base.Services;

public interface IBaseService
{
    public Task<IEnumerable<BaseResponse>> GetAllAsync();
    public Task<BaseResponse?> GetByGuidAsync(string guid);
    public Task<BaseResponse?> GetByTipoAsync(string tipo);
    public Task<BaseResponse> CreateAsync(BaseRequest baseRequest);
    public Task<BaseResponse?> UpdateAsync(string guid, BaseUpdateDto baseUpdate);
    public Task<BaseResponse?> DeleteAsync(string guid);
    public Task<PageResponse<BaseResponse>> GetAllPagedAsync(PageRequest pageRequest);
    
    public Task<BaseModel?> GetBaseModelByGuid(string guid);
    public Task<BaseModel?> GetBaseModelById(long id);
    public Task<IEnumerable<BaseModel>> GetAllForStorage();

}