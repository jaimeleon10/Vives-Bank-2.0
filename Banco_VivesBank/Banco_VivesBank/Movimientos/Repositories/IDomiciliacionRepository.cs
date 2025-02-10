using Banco_VivesBank.Movimientos.Models;

namespace Banco_VivesBank.Movimientos.Repositories;

public interface IDomiciliacionRepository
{
    Task<List<Domiciliacion>> GetAllDomiciliacionesAsync();
    Task<List<Domiciliacion>> GetAllDomiciliacionesActivasAsync();
    Task<Domiciliacion> GetDomiciliacionByIdAsync(string id);
    Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion);
    Task<Domiciliacion> UpdateDomiciliacionAsync(string id, Domiciliacion domiciliacion);
    Task<Domiciliacion> DeleteDomiciliacionAsync(string id);
    Task<List<Domiciliacion>> GetDomiciliacionesActivasByClienteGiudAsync(string clienteGuid);
    Task<List<Domiciliacion>> GetDomiciliacionByClientGuidAsync(string clientGuid);



    
}