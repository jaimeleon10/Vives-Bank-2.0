using Banco_VivesBank.Movimientos.Dto;

namespace Banco_VivesBank.Movimientos.Services.Domiciliaciones;

public interface IDomiciliacionService
{
    public Task<IEnumerable<DomiciliacionResponse>> GetAllAsync();
    public Task<DomiciliacionResponse?> GetByGuidAsync(string domiciliacionGuid);
    public Task<IEnumerable<DomiciliacionResponse>> GetByClienteGuidAsync(string clienteGuid);
    public Task<IEnumerable<DomiciliacionResponse>> GetMyDomiciliaciones(User.Models.User userAuth);

    public Task<DomiciliacionResponse> CreateAsync(User.Models.User userAuth, DomiciliacionRequest domiciliacionRequest);
    
    public Task<DomiciliacionResponse?> DesactivateDomiciliacionAsync(string domiciliacionGuid);
    public Task<DomiciliacionResponse?> DesactivateMyDomiciliacionAsync(User.Models.User userAuth, string domiciliacionGuid);
}