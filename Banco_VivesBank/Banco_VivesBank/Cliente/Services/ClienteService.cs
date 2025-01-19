using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Service;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Cliente.Services;

public class ClienteService : IClienteService
{
    private readonly GeneralDbContext _context;
    private readonly ILogger<ClienteService> _logger;
    private readonly IUserService _userService;

    public ClienteService(GeneralDbContext context, ILogger<ClienteService> logger, IUserService userService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }
    
    public async Task<IEnumerable<ClienteResponse>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los clientes");
		var clientesEntityList = await _context.Clientes.ToListAsync();
        var clienteResponseList = new List<ClienteResponse>();
        foreach (var clienteEntity in clientesEntityList)
        {
            var user = await _userService.GetUserModelById(clienteEntity.UserId);
            var clienteResponse = ClienteMapper.ToResponseFromEntity(clienteEntity, user!);
            clienteResponseList.Add(clienteResponse);
        }

        return clienteResponseList;
    }

    public async Task<ClienteResponse?> GetByGuidAsync(string guid)
    {
		_logger.LogInformation($"Buscando cliente con guid: {guid}");
        var clienteEntity = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            var user = await _userService.GetUserModelById(clienteEntity.UserId);
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return ClienteMapper.ToResponseFromEntity(clienteEntity, user!);
        }
        
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    public async Task<ClienteResponse> CreateAsync(ClienteRequest clienteRequest)
    {
        _logger.LogInformation("Creando cliente");
        
        ValidateDniExistente(clienteRequest.Dni);
        ValidateEmailExistente(clienteRequest.Email);
        ValidateTelefonoExistente(clienteRequest.Telefono);
        
        var user = await _userService.GetUserModelByGuid(clienteRequest.UserGuid);
        if (user == null)
        {
            _logger.LogInformation($"Usuario no encontrado con guid: {clienteRequest.UserGuid}");
            throw new UserNotFoundException($"Usuario no encontrado con guid: {clienteRequest.UserGuid}");
        }

        var clienteModel = ClienteMapper.ToModelFromRequest(clienteRequest, user);
        _context.Clientes.Add(ClienteMapper.ToEntityFromModel(clienteModel)); 
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente creado con éxito");
        return ClienteMapper.ToResponseFromModel(clienteModel);
    }

    public async Task<ClienteResponse?> UpdateAsync(string guid, ClienteRequestUpdate clienteRequestUpdate){
        _logger.LogInformation($"Actualizando cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }
        if(!clienteRequestUpdate.HasAtLeastOneField())
        {
            _logger.LogInformation("No se ha modificado ningún campo");
            return ClienteMapper.ToResponseFromEntity(clienteEntityExistente, await _userService.GetUserModelById(clienteEntityExistente.UserId));
        }

        if (clienteEntityExistente.Dni != clienteRequestUpdate.Dni && !string.IsNullOrEmpty(clienteRequestUpdate.Dni))
        {
            ValidateDniExistente(clienteRequestUpdate.Dni);
        }
        if (clienteEntityExistente.Email != clienteRequestUpdate.Email && !string.IsNullOrEmpty(clienteRequestUpdate.Email))
        {
            ValidateEmailExistente(clienteRequestUpdate.Email);
        }
        if (clienteEntityExistente.Telefono != clienteRequestUpdate.Telefono && !string.IsNullOrEmpty(clienteRequestUpdate.Telefono))
        {
            ValidateTelefonoExistente(clienteRequestUpdate.Telefono);
        }
        var clienteUpdated = ClienteMapper.ToModelFromRequestUpdate(clienteEntityExistente, clienteRequestUpdate);
        
        _context.Clientes.Update(clienteUpdated);
        await _context.SaveChangesAsync();

        var user = await _userService.GetUserModelById(clienteUpdated.UserId);
        _logger.LogInformation($"Cliente actualizado con guid: {guid}");
        return ClienteMapper.ToResponseFromEntity(clienteUpdated, user!);
    }

    public async Task<ClienteResponse?> DeleteByGuidAsync(string guid) 
    {
        _logger.LogInformation($"Borrando cliente con guid: {guid}");
        
        var clienteExistenteEntity = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteExistenteEntity == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        clienteExistenteEntity.IsDeleted = true;
        clienteExistenteEntity.UpdatedAt = DateTime.UtcNow;

        _context.Clientes.Update(clienteExistenteEntity);
        await _context.SaveChangesAsync();

        var user = await _userService.GetUserModelById(clienteExistenteEntity.UserId);
        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {guid}");
        return ClienteMapper.ToResponseFromEntity(clienteExistenteEntity, user!);
    }
    
    private void ValidateDniExistente(string dni)
    {
        _logger.LogInformation("Validando Dni");
        if (_context.Clientes.Any(c => c.Dni.ToLower() == dni.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el DNI: {dni}");
            throw new ClienteExistsException($"Ya existe un cliente con el DNI: {dni}");
        }
    }
    private void ValidateEmailExistente(string email)
    {
        _logger.LogInformation("Validando email");
        if(_context.Clientes.Any(c => c.Email.ToLower() == email.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el email: {email}");
            throw new ClienteExistsException($"Ya existe un cliente con el email: {email}");
        }
    }
    private void ValidateTelefonoExistente(string telefono)
    {
        _logger.LogInformation("Validando teléfono");
        if(_context.Clientes.Any(c => c.Telefono == telefono))
        {
            _logger.LogInformation($"Ya existe un cliente con el teléfono: {telefono}");
            throw new ClienteExistsException($"Ya existe un cliente con el teléfono: {telefono}");
        }
    }
}
