using Vives_Bank_Net.Rest.Cliente.Dtos;

namespace Vives_Bank_Net.Rest.Cliente.Services;

public class ClienteService : IClienteService
{
    public Task<List<ClienteResponse>> GetAllClientesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ClienteResponse> GetClienteByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<ClienteResponse> CreateClienteAsync(ClienteRequestSave requestSave)
    {
        throw new NotImplementedException();
    }

    public Task<ClienteResponse> UpdateClienteAsync(string id, ClienteRequestUpdate requestUpdate)
    {
        throw new NotImplementedException();
    }

    public Task<ClienteResponse> DeleteClienteAsync(string id)
    {
        throw new NotImplementedException();
    }
}