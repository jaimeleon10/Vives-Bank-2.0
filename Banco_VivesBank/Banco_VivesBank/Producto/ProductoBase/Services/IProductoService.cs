using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.ProductoBase.Services;

public interface IProductoService
{
    public Task<PageResponse<ProductoResponse>> GetAllPagedAsync(PageRequest pageRequest);
    public Task<ProductoResponse?> GetByGuidAsync(string guid);
    public Task<ProductoResponse?> GetByTipoAsync(string tipo);
    public Task<ProductoResponse> CreateAsync(ProductoRequest productoRequest);
    public Task<ProductoResponse?> UpdateAsync(string guid, ProductoRequestUpdate productoRequestUpdate);
    public Task<ProductoResponse?> DeleteByGuidAsync(string guid);
    
    public Task<ProductoBase.Models.Producto?> GetBaseModelByGuid(string guid);
    public Task<ProductoBase.Models.Producto?> GetBaseModelById(long id);
    
    public Task<IEnumerable<ProductoBase.Models.Producto>> GetAllForStorage();
}