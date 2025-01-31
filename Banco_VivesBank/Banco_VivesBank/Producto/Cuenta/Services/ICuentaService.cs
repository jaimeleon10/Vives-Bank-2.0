using System.Numerics;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public interface ICuentaService
{
    public Task<PageResponse<CuentaResponse>> GetAllAsync(
        double? saldoMax, 
        double? saldoMin,
        String? tipoCuenta, 
        PageRequest pageRequest);
    
    public Task<IEnumerable<CuentaResponse>> GetByClientGuidAsync(string guid);
    public Task<IEnumerable<CuentaResponse>> GetAllMeAsync(string guid);

    public Task<CuentaResponse?> GetByGuidAsync(string guid);
    
    public Task<CuentaResponse?> GetByIbanAsync(string iban);

    public Task<CuentaResponse?> GetByTarjetaGuidAsync(string tarjetaGuid);

    public Task<CuentaResponse?> GetMeByIbanAsync(string guid, string iban);

    public Task<Models.Cuenta?> GetCuentaModelByGuidAsync(string guid);
     public Task<Models.Cuenta?> GetCuentaModelByIdAsync(long id);
    
    public Task<CuentaResponse> CreateAsync(string guid,CuentaRequest cuentaRequest);

    public Task<CuentaResponse?> DeleteMeAsync(string guidClient,string guid);
    
    public Task<CuentaResponse?> DeleteByGuidAsync(string guid);

    public Task<List<Models.Cuenta>> GetAllForStorage();
}
