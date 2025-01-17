using Microsoft.EntityFrameworkCore;
using Vives_Bank_Net.Rest.Cliente.Database;
using Vives_Bank_Net.Rest.Cliente.Dtos;
using Vives_Bank_Net.Rest.Cliente.Exceptions;
using Vives_Bank_Net.Rest.Cliente.Mapper;
using Vives_Bank_Net.Rest.Database;

namespace Vives_Bank_Net.Rest.Cliente.Services;

public class ClienteService : IClienteService
{
    
    private readonly GeneralDbContext _context;
    private readonly ClienteMapper _clienteMapper;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(GeneralDbContext context, ClienteMapper clienteMapper, ILogger<ClienteService> logger)
    {
        _context = context;
        _clienteMapper = clienteMapper;
        _logger = logger;
    }
    
    public async Task<List<ClienteResponse>> GetAllClientesAsync()
    {
        _logger.LogInformation("Buscando todos los clientes");
		List<ClienteEntity> clientes = await _context.Clientes.ToListAsync();

        var clienteResponses =  clientes.Select(c => _clienteMapper.FromEntityClienteToResponse(c)).ToList();
        return clienteResponses;
    }

    public async Task<ClienteResponse> GetClienteByIdAsync(string id)
    {
		_logger.LogInformation($"Buscando cliente con id '{id}'.");
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == id);
        if (cliente != null)
        {
            return _clienteMapper.FromEntityClienteToResponse(cliente);
        }
        throw new ClienteNotFound($"Cliente con id '{id}' no existe.");
    }

    public async Task<ClienteResponse> CreateClienteAsync(ClienteRequestSave requestSave)
    {
        _logger.LogInformation("Guardando un nuevo cliente");

        await ValidateClienteExistente(requestSave.Dni, requestSave.Email, requestSave.Telefono);

        var clienteEntity = _clienteMapper.FromSaveDtoToClienteEntity(requestSave);
        _context.Clientes.Add(clienteEntity); 
        await _context.SaveChangesAsync();
        
        return _clienteMapper.FromEntityClienteToResponse(await FindClienteEntity(clienteEntity.Guid));
    }

    public async Task<ClienteResponse> UpdateClienteAsync(string id, ClienteRequestUpdate requestUpdate){
        _logger.LogInformation($"Actualizando cliente con id {id}");
        var clienteEntity = await FindClienteEntity(id);
        await ValidateClienteExistente(null, requestUpdate?.Email, requestUpdate?.Telefono);
        if (requestUpdate == null || !requestUpdate.HasAtLeastOneField())
            throw new ClienteBadRequest("Debe cambiar al menos un dato para realizar la actualización.");
        var updatedCliente = _clienteMapper.FromUpdateDtoToClienteEntity(clienteEntity, requestUpdate); 
        _context.Clientes.Update(updatedCliente);
        await _context.SaveChangesAsync();
        return _clienteMapper.FromEntityClienteToResponse(await FindClienteEntity(id));

    }

    public async Task<ClienteResponse> DeleteClienteAsync(string guid) 
    {
        _logger.LogInformation($"Borrando cliente con id: {guid}");
        var entityCliente = await FindClienteEntity(guid);

        var deletedCliente = DeleteData(entityCliente);
        _context.Clientes.Update(deletedCliente);
        await _context.SaveChangesAsync();
        return _clienteMapper.FromEntityClienteToResponse(await FindClienteEntity(guid));
    }


    public ClienteEntity DeleteData(ClienteEntity entityCliente)
    {
        entityCliente.Dni = "null";
        entityCliente.Nombre = "null";
        entityCliente.Apellidos = "null";
        entityCliente.Direccion.Calle = "null";
        entityCliente.Direccion.Numero = "null";
        entityCliente.Direccion.CodigoPostal = "null";
        entityCliente.Direccion.Piso = "null";
        entityCliente.Direccion.Letra = "null";
        entityCliente.Email = "null";
        entityCliente.Telefono = "0";
        entityCliente.FotoPerfil= "null";
        entityCliente.FotoDni = "null";
        entityCliente.IsDeleted = true;
        entityCliente.UpdatedAt = DateTime.UtcNow;
        //antes de borrar los urls borrar en volumen los recursos con el servicio de storage

        return entityCliente;
    }

    public async Task ValidateClienteExistente(string? dni, string? email, string? telefono){
        if (await _context.Clientes.AnyAsync(c => c.Dni == dni))
        {
            throw new ClienteConflict($"Ya existe un cliente con el DNI: {dni}");
        }
        if(await _context.Clientes.AnyAsync(c => c.Email == email))
        {
            throw new ClienteConflict($"Ya existe un cliente con el email: {email}");
        }
        if(await _context.Clientes.AnyAsync(c => c.Telefono == telefono))
        {
            throw new ClienteConflict($"Ya existe un cliente con el teléfono: {telefono}");
        }
    }

    public async Task<ClienteEntity> FindClienteEntity(string guid)
    {
        var entityCliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if(entityCliente == null)
        {
            throw new ClienteNotFound($"No se encontró el cliente con id: {guid}");
        }
        return entityCliente;
    }
}
