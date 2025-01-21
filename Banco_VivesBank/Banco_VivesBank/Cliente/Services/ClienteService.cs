using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Mappers;
using Banco_VivesBank.Storage.Files.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Service;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Cliente.Services;

public class ClienteService : IClienteService
{
    private readonly GeneralDbContext _context;
    private readonly ILogger<ClienteService> _logger;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;

    public ClienteService(GeneralDbContext context, ILogger<ClienteService> logger, IUserService userService, IFileStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
        _fileStorageService = storageService;
    }
    
    public async Task<IEnumerable<ClienteResponse>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los clientes");
		var clientesEntityList = await _context.Clientes.Include(c=> c.User).ToListAsync();
        var clienteResponseList = new List<ClienteResponse>();
        foreach (var clienteEntity in clientesEntityList)
        {
            var clienteResponse = ClienteMapper.ToResponseFromEntity(clienteEntity);
            clienteResponseList.Add(clienteResponse);
        }

        return clienteResponseList;
    }

    public async Task<ClienteResponse?> GetByGuidAsync(string guid)
    {
		_logger.LogInformation($"Buscando cliente con guid: {guid}");
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return ClienteMapper.ToResponseFromEntity(clienteEntity);
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
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }
        if(!clienteRequestUpdate.HasAtLeastOneField())
        {
            _logger.LogInformation("No se ha modificado ningún campo");
            return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
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

        _logger.LogInformation($"Cliente actualizado con guid: {guid}");
        return ClienteMapper.ToResponseFromEntity(clienteUpdated);
    }

    public async Task<ClienteResponse?> DeleteByGuidAsync(string guid) 
    {
        _logger.LogInformation($"Borrando cliente con guid: {guid}");
        
        var clienteExistenteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteExistenteEntity == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        clienteExistenteEntity.IsDeleted = true;
        clienteExistenteEntity.UpdatedAt = DateTime.UtcNow;

        _context.Clientes.Update(clienteExistenteEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {guid}");
        return ClienteMapper.ToResponseFromEntity(clienteExistenteEntity);
    }

    public async Task<string> DerechoAlOlvido(string userGuid)
    {
        _logger.LogInformation($"Borrando cliente del usuario con guid: {userGuid}");
        
        var user = await _userService.GetUserModelByGuid(userGuid);
        if (user == null)
        {
            _logger.LogInformation($"Usuario no encontrado con guid: {userGuid}");
            throw new UserNotFoundException($"Usuario no encontrado con guid: {userGuid}");
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == user.Id);
        
        cliente = DeleteData(cliente!);
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Datos del cliente eliminados de la base de datos");
        return "Datos del cliente eliminados de la base de datos";
    }
    
    private ClienteEntity DeleteData(ClienteEntity entityCliente)
    {
        entityCliente.Dni = "";
        entityCliente.Nombre = "";
        entityCliente.Apellidos = "";
        entityCliente.Direccion.Calle = "";
        entityCliente.Direccion.Numero = "";
        entityCliente.Direccion.CodigoPostal = "";
        entityCliente.Direccion.Piso = "";
        entityCliente.Direccion.Letra = "";
        entityCliente.Email = "";
        entityCliente.Telefono = "";
        _fileStorageService.DeleteFileAsync(entityCliente.FotoPerfil);
        _fileStorageService.DeleteFileAsync(entityCliente.FotoDni);
        entityCliente.FotoPerfil= "";
        entityCliente.FotoDni = "";
        entityCliente.IsDeleted = true;
        entityCliente.UpdatedAt = DateTime.Now;

        return entityCliente;
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
    
    public async Task<ClienteResponse?> UpdateFotoPerfil(string guid, IFormFile fotoPerfil)
    {
        _logger.LogInformation($"Actualizando foto de perfil del cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        var fotoAnterior = clienteEntityExistente.FotoPerfil;
        if (clienteEntityExistente.FotoPerfil != "https://example.com/fotoPerfil.jpg")
        {
            await _fileStorageService.DeleteFileAsync(fotoAnterior);
            
        }
        var nuevaFoto = await _fileStorageService.SaveFileAsync(fotoPerfil);
        clienteEntityExistente.FotoPerfil = nuevaFoto;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
    }

    public async Task<ClienteResponse?> UpdateFotoDni(string guid, IFormFile fotoDni)
    {
        _logger.LogInformation($"Actualizando foto del DNI del cliente con guid: {guid}");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
            return null;
        }

        var fotoAnterior = clienteEntityExistente.FotoDni;
        if (clienteEntityExistente.FotoDni != "https://example.com/fotoDni.jpg")
        {
            await _fileStorageService.DeleteFileAsync(fotoAnterior);
            
        }
        var nuevaFoto = await _fileStorageService.SaveFileAsync(fotoDni);
        clienteEntityExistente.FotoDni= nuevaFoto;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        return ClienteMapper.ToResponseFromEntity(clienteEntityExistente);
    }
    
    public async Task<Models.Cliente?> GetClienteModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cliente con guid: {guid}");

        var clienteEntity = await _context.Clientes.FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return ClienteMapper.ToModelFromEntity(clienteEntity);
        }

        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }
        
    public async Task<Models.Cliente?> GetClienteModelById(long id)
    {
        _logger.LogInformation($"Buscando Cliente con id: {id}");

        var clienteEntity = await _context.Clientes.FirstOrDefaultAsync(t => t.Id == id);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con id: {id}");
            return ClienteMapper.ToModelFromEntity(clienteEntity);
        }

        _logger.LogInformation($"Cliente no encontrado con id: {id}");
        return null;
    }
    
    public async Task<IEnumerable<ClienteEntity>> GetAllModelsAsync()
    {
        _logger.LogInformation("Obteniendo todos los clientes modelos");
        var clientesEntityList = await _context.Clientes.Include(c=>c.User).ToListAsync();
        var clienteResponseList = new List<ClienteEntity>();
        foreach (var clienteEntity in clientesEntityList)
        {
            clienteResponseList.Add(clienteEntity);
        }

        return clienteResponseList;
    }
    
}
