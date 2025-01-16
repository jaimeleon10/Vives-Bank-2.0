using Vives_Bank_Net.Rest.Cliente.Dtos;

namespace Vives_Bank_Net.Rest.Cliente.Services;

public interface IClienteService
{
    public Task<List<ClienteResponse>> GetAllClientesAsync();
    public Task<ClienteResponse> GetClienteByIdAsync(string id);
    public Task<ClienteResponse> CreateClienteAsync(ClienteRequestSave requestSave);
    public Task<ClienteResponse> UpdateClienteAsync(string id, ClienteRequestUpdate requestUpdate);
    public Task<ClienteResponse> DeleteClienteAsync(string id);
}