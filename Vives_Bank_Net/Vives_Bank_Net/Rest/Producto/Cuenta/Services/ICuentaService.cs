using System.Numerics;
using Vives_Bank_Net.Rest.Producto.Cuenta.Dto;
using VivesBankProject.Utils.Pagination;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Services;

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
    
    public Task<CuentaResponse> save(CuentaRequest cuentaRequest);
    
    public Task<CuentaResponse> update(string guid, CuentaUpdateRequest cuentaRequest);
    
    public Task<CuentaResponse> delete(string guid);
}