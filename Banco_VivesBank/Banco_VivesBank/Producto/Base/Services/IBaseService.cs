﻿using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Base.Models;

namespace Banco_VivesBank.Producto.Base.Services;

public interface IBaseService
{
    public Task<IEnumerable<BaseResponse>> GetAllAsync();
    public Task<BaseResponse?> GetByGuidAsync(string guid);
    public Task<BaseResponse?> GetByTipoAsync(string tipo);
    public Task<BaseResponse> CreateAsync(BaseRequest baseRequest);
    public Task<BaseResponse?> UpdateAsync(string guid, BaseUpdateDto baseUpdate);
    public Task<BaseResponse?> DeleteAsync(string guid);
}