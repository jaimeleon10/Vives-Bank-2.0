using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Cliente.Services;

public interface IClienteService
{
    public Task<PageResponse<ClienteResponse>> GetAllPagedAsync(string? nombre, string? apellido, string? dni, PageRequest pageRequest);
    public Task<ClienteResponse?> GetByGuidAsync(string guid);
    public Task<ClienteResponse?> GetMeAsync(User.Models.User userAuth);
    
    public Task<ClienteResponse> CreateAsync(User.Models.User userAuth, ClienteRequest request);
    
    public Task<ClienteResponse?> UpdateMeAsync(User.Models.User userAuth, ClienteRequestUpdate requestUpdate);
    
    public Task<ClienteResponse?> DeleteByGuidAsync(string guid);
    public Task<ClienteResponse?> DeleteMeAsync(User.Models.User userAuth);

    public Task<string> DerechoAlOlvido(User.Models.User userAuth);
    
    public Task<ClienteResponse?> UpdateFotoDni(User.Models.User userAuth, IFormFile dniFoto);
    public Task<Stream> GetFotoDniAsync(string guid);
    public Task<ClienteResponse?> UpdateFotoPerfil(User.Models.User userAuth, IFormFile fotoPerfil);
    public Task<IEnumerable<Models.Cliente>> GetAllForStorage();

    public Task<Models.Cliente?> GetClienteModelByGuid(string guid);
    public Task<Models.Cliente?> GetClienteModelById(long id);
}