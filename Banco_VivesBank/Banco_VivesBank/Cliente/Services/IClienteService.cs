using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Cliente.Services;

public interface IClienteService
{
    public Task<IEnumerable<ClienteResponse>> GetAllAsync();
    public Task<ClienteResponse?> GetByGuidAsync(string guid);
    public Task<ClienteResponse> CreateAsync(ClienteRequest request);
    public Task<ClienteResponse?> UpdateAsync(string guid, ClienteRequestUpdate requestUpdate);
    public Task<ClienteResponse?> DeleteByGuidAsync(string guid);
    public Task<string> DerechoAlOlvido(string userGuid);
    public Task<ClienteResponse?> UpdateFotoDni(string guid, IFormFile dniFoto);
    public Task<ClienteResponse?> UpdateFotoPerfil(string guid, IFormFile fotoPerfil);
    
    public Task<Models.Cliente?> GetClienteModelByGuid(string guid);
    public Task<Models.Cliente?> GetClienteModelById(long id);
    
    public Task<PageResponse<ClienteResponse>> GetAllPagedAsync(string? nombre, string? apellido, string? dni, PageRequest pageRequest);
}