using Banco_VivesBank.Cliente.Dto;

namespace Banco_VivesBank.Cliente.Services;

public interface IClienteService
{
    public Task<IEnumerable<ClienteResponse>> GetAllAsync();
    public Task<ClienteResponse?> GetByGuidAsync(string guid);
    public Task<ClienteResponse> CreateAsync(ClienteRequest request);
    public Task<ClienteResponse?> UpdateAsync(string guid, ClienteRequestUpdate requestUpdate);
    public Task<ClienteResponse?> DeleteByGuidAsync(string guid);
}