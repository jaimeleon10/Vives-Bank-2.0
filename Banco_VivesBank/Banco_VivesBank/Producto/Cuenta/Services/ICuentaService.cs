﻿using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public interface ICuentaService
{
    public Task<PageResponse<CuentaResponse>> GetAllAsync(
        BigInteger? saldoMax, 
        BigInteger? saldoMin,
        String? tipoCuenta, 
        PageRequest pageRequest);
    
    public Task<IEnumerable<CuentaResponse>> GetByClientGuidAsync(string guid);

    public Task<CuentaResponse?> GetByGuidAsync(string guid);
    
    public Task<CuentaResponse?> GetByIbanAsync(string iban);

    //public Task<CuentaResponse?> GetMeByIbanAsync(string guid, string iban);

    public Task<Models.Cuenta?> GetCuentaModelByGuid(string guid);
     public Task<Models.Cuenta?> GetCuentaModelById(long id);
    
    //public Task<CuentaResponse> CreateAsync(string guid,CuentaRequest cuentaRequest);
    public Task<CuentaResponse> CreateAsync(CuentaRequest cuentaRequest);

    
    //public Task<CuentaResponse?> UpdateAsync(string guidClient,string guid, CuentaUpdateRequest cuentaRequest);
    public Task<CuentaResponse?> UpdateAsync(string guid, CuentaUpdateRequest cuentaRequest);

    //public Task<CuentaResponse?> DeleteAsync(string guidClient,string guid);
    
    public Task<CuentaResponse?> DeleteAdminAsync(string guid);
}
