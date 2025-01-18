using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public interface ICuentaService
{
    public Task<PageResponse<CuentaResponse>> GetAll(
        BigInteger? saldoMax, 
        BigInteger? saldoMin,
        String? tipoCuenta, 
        PageRequest pageRequest);
    
    public Task<List<CuentaResponse>> getByClientGuid(string guid);

    public Task<CuentaResponse> getByGuid(string guid);
    
    public Task<CuentaResponse> getByIban(string iban);

    public Task<CuentaResponse> getMeByIban(string guid, string iban);
    
    public Task<CuentaResponse> save(string guid,CuentaRequest cuentaRequest);
    
    public Task<CuentaResponse> update(string guidClient,string guid, CuentaUpdateRequest cuentaRequest);
    
    public Task<CuentaResponse> delete(string guidClient,string guid);
    
    public Task<CuentaResponse> deleteAdmin(string guid);
}
