using System.Runtime.Serialization;
using System.Text.Json;
using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Exceptions;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Storage.Ftp.Service;
using Banco_VivesBank.Storage.Images.Exceptions;
using Banco_VivesBank.Storage.Images.Service;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Mapper;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using Path = System.IO.Path;
using Role = Banco_VivesBank.User.Models.Role;

namespace Banco_VivesBank.Cliente.Services;

/// <summary>
///  Servicio que gestiona las operaciones relacionadas con los clientes.
/// </summary>
public class ClienteService : IClienteService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly GeneralDbContext _context;
    private readonly ILogger<ClienteService> _logger;
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFtpService _ftpService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _redisDatabase;
    private const string CacheKeyPrefix = "Cliente:";

    public ClienteService(GeneralDbContext context, ILogger<ClienteService> logger, IUserService userService, IFileStorageService storageService, IMemoryCache memoryCache, IConnectionMultiplexer redis, IFtpService ftpService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
        _fileStorageService = storageService;
        _memoryCache = memoryCache;
        _redis = redis;
        _redisDatabase = _redis.GetDatabase();
        _ftpService = ftpService;
    }

    /// <summary>
    /// Paginacion y filtrado de clientes en la base de datos.
    /// </summary>
    /// <remarks>Busca los clientes dependiendo de los datos a filtrar introducidos por el cliente, se puede modificar la direccion y por que se ordenan los clientes
    /// finalmente crea una pagina a partir de los datos de page y devuelve los datos</remarks>
    /// <param name="nombre">Nombre a filtrar</param>
    /// <param name="apellidos">Apellidos a filtrar</param>
    /// <param name="dni">Dni a filtrar</param>
    /// <param name="page">Atributos para crear la página con los clientes</param>
    /// <returns>Un PageResponse con los datos de los clientes encontrados bajo los filtros</returns>
    /// <exception cref="InvalidOperationException"> Se lanza la excepcion cuando se intenta ordenar por un valor no admintido</exception>
    public async Task<PageResponse<ClienteResponse>> GetAllPagedAsync(string? nombre, string? apellidos, string? dni, PageRequest page)
    {
        _logger.LogInformation("Obteniendo todos los clientes paginados y filtrados");
        int pageNumber = page.PageNumber >= 0 ? page.PageNumber : 0;
        int pageSize = page.PageSize > 0 ? page.PageSize : 10;

        var query = _context.Clientes.Include(c => c.User).AsQueryable();

        query = page.SortBy.ToLower() switch
        {
            "nombre" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Nombre) 
                : query.OrderByDescending(c => c.Nombre),
            "apellidos" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Apellidos) 
                : query.OrderByDescending(c => c.Apellidos),
            "dni" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Dni) 
                : query.OrderByDescending(c => c.Dni),
            "id" => page.Direction.ToUpper() == "ASC" 
                ? query.OrderBy(c => c.Id) 
                : query.OrderByDescending(c => c.Id),
            _ => throw new InvalidOperationException($"La propiedad {page.SortBy} no es válida para ordenamiento.")
        };
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            _logger.LogInformation($"Filtrando clientes por nombre {nombre}");;
            query = query.Where(c => c.Nombre.ToLower().Contains(nombre.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(apellidos))
        {  
           _logger.LogInformation($"Filtrando clientes por apellidos {apellidos}");
            query = query.Where(c => c.Apellidos.ToLower().Contains(apellidos.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(dni))
        {
            _logger.LogInformation($"Filtrando clientes por dni {dni}");
            query = query.Where(c => c.Dni.ToLower().Contains(dni.ToLower()));
        }

        query = query.OrderBy(c => c.Id);

        var totalElements = await query.CountAsync();
        
        var content = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();
        
        var totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
        
        var contentResponse = content.Select(ClienteMapper.ToResponseFromEntity).ToList();
        
        return new PageResponse<ClienteResponse>
        {
            Content = contentResponse,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalElements = totalElements,
            TotalPages = totalPages,
            Empty = !content.Any(),
            First = pageNumber == 0,
            Last = pageNumber == totalPages - 1,
            SortBy = page.SortBy,
            Direction = page.Direction
        };
    }

    /// <summary>
    /// Busca un cliente a partir de un guid.
    /// </summary>
    /// <remarks>Se busca primero en el cache en memoria, despues en el cache redis y finalmente en la base de datos,
    /// los datos buscados se almacenan en ambas caches, devuelve un null si no se encuentra ningun cliente</remarks>
    /// <param name="guid">Identificador del cliente</param>
    /// <returns>Devuelve un ClientResponse en caso de que se encuentre</returns>
    /// <exception cref="Exception">Lanza un excepcion genérica en caso de que ocurra algun error al serializar el cliente en redis</exception>
    public async Task<ClienteResponse?> GetByGuidAsync(string guid)
    {
		_logger.LogInformation($"Buscando cliente con guid: {guid}");

        var cacheKey = CacheKeyPrefix + guid;
        
        if (_memoryCache.TryGetValue(cacheKey, out Models.Cliente? cachedCliente))
        {
            _logger.LogInformation("Cliente obtenido desde cache en memoria");
            return cachedCliente!.ToResponseFromModel();
        }
        
        var redisCacheCliente = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheCliente.IsNullOrEmpty)
        {
            _logger.LogInformation("Cliente obtenido desde cache en Redis");
            var clienteFromRedis = JsonSerializer.Deserialize<Models.Cliente>(redisCacheCliente!);
            if (clienteFromRedis == null)
            {
                _logger.LogWarning("Error al deserializar usuario desde Redis");
                throw new Exception("Error al deserializar usuario desde Redis");
            }
            
            _memoryCache.Set(cacheKey, clienteFromRedis, TimeSpan.FromMinutes(30));
            return clienteFromRedis.ToResponseFromModel();
        }
        
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            var clienteResponse = clienteEntity.ToResponseFromEntity();
            var clienteModel = clienteEntity.ToModelFromEntity();

            _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));

            var redisValue = JsonSerializer.Serialize(clienteResponse);
            await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return clienteResponse;
        }
        
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    /// <summary>
    /// Busca los datos de un cliente autenticado.
    /// </summary>
    /// <remarks>El usuario debe estar relacionado con el cliente correspondiente, en caso de que no se encuentre un cliente asociado al usuario se devuelve un null</remarks>
    /// <param name="userAuth">Usuario que esta buscando sus datos</param>
    /// <returns>Devuelve los datos del cliente asociado al usuario</returns>
    public async Task<ClienteResponse?> GetMeAsync(User.Models.User userAuth)
    {
        _logger.LogInformation($"Buscando cliente autenticado");
        var clienteExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Id == userAuth.Id);
        if (clienteExistente == null)
        {
            _logger.LogWarning($"No se ha encontrado el cliente autenticado");
            return null;
        }

        _logger.LogInformation($"Se ha encontrado el cliente con guid {clienteExistente.Guid} correspondiente al usuario con guid {userAuth.Guid}");
        return clienteExistente.ToResponseFromEntity();
    }

    /// <summary>
    /// Crea un cliente a partir de los datos introducidos por el usuario
    /// </summary>
    /// <remarks>Los datos como dni, email y telefono, no pueden estar asociados a otro cliente, en ese caso se devuelve una ClienteExistsException
    /// En caso de que el usuario ya tenga un cleiente asociado se lanza la misma excepcion
    /// Si es correcto se guarda el cliente y se cambia el rol del usuario a Cliente, tambien se almacenan los datos del cliente en la cache </remarks>
    /// <param name="userAuth">Usuario que esta intentando crear su cliente</param>
    /// <param name="clienteRequest">Datos del cliente a almacenar</param>
    /// <returns>Los datos del nuevo cliente almacenado</returns>
    /// <exception cref="ClienteExistsException">Se lanza en caso de que el usuario ya tenga un cliente relacionado o existan dni telefono o email en otro cliente</exception>
    public async Task<ClienteResponse> CreateAsync(User.Models.User userAuth, ClienteRequest clienteRequest)
    {
        _logger.LogInformation("Creando cliente");
        
        ValidateDniExistente(clienteRequest.Dni);
        ValidateEmailExistente(clienteRequest.Email);
        ValidateTelefonoExistente(clienteRequest.Telefono);
        
        _logger.LogInformation($"Validando que el usuario con guid {userAuth.Guid} no sea ya un cliente");
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteEntityExistente != null)
        {
            _logger.LogWarning($"El usuario con guid {userAuth.Guid} ya es un cliente");
            throw new ClienteExistsException($"El usuario con guid {userAuth.Guid} ya es un cliente");
        }
        
        _logger.LogInformation($"Actualizando rol a cliente del usuario con guid {userAuth.Guid}");
        userAuth.Role = Role.Cliente;
        _context.Usuarios.Update(userAuth.ToEntityFromModel());
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Guardando cliente en base de datos");
        var clienteModel = clienteRequest.ToModelFromRequest(userAuth);
        var clienteEntity = clienteModel.ToEntityFromModel();
        _context.Clientes.Add(clienteEntity); 
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Guardando cliente con guid {clienteModel.Guid} en caché");
        var cacheKey = CacheKeyPrefix + clienteModel.Guid;
        var serializedCliente = JsonSerializer.Serialize(clienteModel);
        _memoryCache.Set(cacheKey, clienteModel, TimeSpan.FromMinutes(30));
        await _redisDatabase.StringSetAsync(cacheKey, serializedCliente, TimeSpan.FromMinutes(30));
        
        _logger.LogInformation("Cliente creado con éxito");
        return clienteModel.ToResponseFromModel();
    }

    /// <summary>
    /// Actualiza los datos de un cliente asociado al usuario autenticado a  partir de los datos introducidos
    /// </summary>
    /// <remarks>El usuario tiene que estar relacionado con un cliente y los datos deben de ser correctos (email, dni y telefono unicos),
    /// en caso de que sean incorrectos lanza una excepcion ClienteExistsException</remarks>
    /// <param name="userAuth">Usuario autenticado con rol Cliente</param>
    /// <param name="clienteRequest">Datos a modificar del cliente</param>
    /// <returns>Devuelve los datos del cliente modificados</returns>
    public async Task<ClienteResponse?> UpdateMeAsync(User.Models.User userAuth, ClienteRequestUpdate clienteRequest){
        _logger.LogInformation($"Actualizando cliente autenticado");
        
        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente autenticado no encontrado");
            return null;
        }

        if (clienteEntityExistente.Dni.ToLower() != clienteRequest.Dni.ToLower())
        {
            ValidateDniExistente(clienteRequest.Dni);
        }
        
        if (clienteEntityExistente.Email.ToLower() != clienteRequest.Email.ToLower())
        {
            ValidateEmailExistente(clienteRequest.Email);
        }
        
        if (clienteEntityExistente.Telefono != clienteRequest.Telefono)
        {
            ValidateTelefonoExistente(clienteRequest.Telefono);
        }

        clienteEntityExistente.Dni = clienteRequest.Dni;
        clienteEntityExistente.Nombre = clienteRequest.Nombre;
        clienteEntityExistente.Apellidos = clienteRequest.Apellidos;
        clienteEntityExistente.Direccion = new Direccion
        {
            Calle = clienteRequest.Calle,
            Numero = clienteRequest.Numero,
            CodigoPostal = clienteRequest.CodigoPostal,
            Piso = clienteRequest.Piso,
            Letra = clienteRequest.Letra
        };
        clienteEntityExistente.Email = clienteRequest.Email;
        clienteEntityExistente.Telefono = clienteRequest.Telefono;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        clienteEntityExistente.IsDeleted = clienteRequest.IsDeleted;
        
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteEntityExistente.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        var clienteModel = clienteEntityExistente.ToModelFromEntity();
        var redisValue = JsonSerializer.Serialize(clienteModel);
        await _redisDatabase.StringSetAsync(cacheKey, redisValue, TimeSpan.FromMinutes(30));

        _logger.LogInformation($"Cliente actualizado con guid: {clienteEntityExistente.Guid}");
        return clienteEntityExistente.ToResponseFromEntity();
    }

    /// <summary>
    /// Borra los datos de un cliente a partir de un guid
    /// </summary>
    /// <param name="guid">Identificador</param>
    /// <returns>Devuelve los datos del cliente con el atributo IsDeleted con valor true</returns>
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
        
        _logger.LogInformation($"Desactivando cuentas y tarjetas pertenecientes al cliente con guid {clienteExistenteEntity.Guid}");
        var cuentas = await _context.Cuentas.Where(c => c.ClienteId == clienteExistenteEntity.Id).ToListAsync();
        foreach (var cuenta in cuentas)
        {
            cuenta.IsDeleted = true;
            _context.Cuentas.Update(cuenta);

            if (cuenta.TarjetaId == null) continue;
            var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuenta.TarjetaId);
            tarjetaExistente!.IsDeleted = true;
            
            _context.Tarjetas.Update(tarjetaExistente);
        }
        
        _logger.LogInformation("Guardando todos los cambios en la base de datos");
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteExistenteEntity.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {guid}");
        return clienteExistenteEntity.ToResponseFromEntity();
    }
    
    /// <summary>
    /// Permite a un usuario borrar su cliente asociado, junto a sus tarjetas y cuentas
    /// </summary>
    /// <param name="userAuth">Usuario con rol Cliente autenticado</param>
    /// <returns>Los datos del cliente a borrar</returns>
    public async Task<ClienteResponse?> DeleteMeAsync(User.Models.User userAuth) 
    {
        _logger.LogInformation($"Borrando cliente autenticado");
        
        var clienteExistenteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteExistenteEntity == null)
        {
            _logger.LogInformation($"Cliente autenticado no encontrado ");
            return null;
        }

        clienteExistenteEntity.IsDeleted = true;
        clienteExistenteEntity.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteExistenteEntity);
        
        _logger.LogInformation($"Desactivando cuentas y tarjetas pertenecientes al cliente con guid {clienteExistenteEntity.Guid}");
        var cuentas = await _context.Cuentas.Where(c => c.ClienteId == clienteExistenteEntity.Id).ToListAsync();
        foreach (var cuenta in cuentas)
        {
            cuenta.IsDeleted = true;
            cuenta.UpdatedAt = DateTime.UtcNow;
            _context.Cuentas.Update(cuenta);

            if (cuenta.TarjetaId == null) continue;
            var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuenta.TarjetaId);
            tarjetaExistente!.IsDeleted = true;
            tarjetaExistente!.UpdatedAt = DateTime.UtcNow;
            
            _context.Tarjetas.Update(tarjetaExistente);
        }
        
        _logger.LogInformation("Guardando todos los cambios en la base de datos");
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteExistenteEntity.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);
        
        _logger.LogInformation($"Cliente borrado (desactivado) con guid: {clienteExistenteEntity.Guid}");
        return clienteExistenteEntity.ToResponseFromEntity();
    }
    /// <summary>
    /// Borra los datos de un cliente, si el usuario esta autenticado y tiene rol de cliente
    /// </summary>
    /// <remarks>Se eliminan tambien los datos en las caches y se ponen en blanco todos los campos</remarks>
    /// <param name="userAuth">Usuario autenticado</param>
    /// <returns>Devuelve un mensaje confirmando que sus datos han sido borrados</returns>
    public async Task<string> DerechoAlOlvido(User.Models.User userAuth)
    {
        _logger.LogInformation($"Borrando cliente autenticado");
        
        var clienteExistenteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteExistenteEntity == null)
        {
            _logger.LogInformation($"Cliente autenticado no encontrado ");
            return null;
        }
        
        _logger.LogInformation($"Desactivando cuentas y tarjetas pertenecientes al cliente con guid {clienteExistenteEntity.Guid}");
        var cuentas = await _context.Cuentas.Where(c => c.ClienteId == clienteExistenteEntity.Id).ToListAsync();
        foreach (var cuenta in cuentas)
        {
            cuenta.IsDeleted = true;
            cuenta.UpdatedAt = DateTime.UtcNow;
            _context.Cuentas.Update(cuenta);

            if (cuenta.TarjetaId == null) continue;
            var tarjetaExistente = await _context.Tarjetas.FirstOrDefaultAsync(t => t.Id == cuenta.TarjetaId);
            tarjetaExistente!.IsDeleted = true;
            tarjetaExistente!.UpdatedAt = DateTime.UtcNow;
            
            _context.Tarjetas.Update(tarjetaExistente);
        }
        
        clienteExistenteEntity = await DeleteData(clienteExistenteEntity!);
        _context.Clientes.Update(clienteExistenteEntity);
        await _context.SaveChangesAsync();
        
        var cacheKey = CacheKeyPrefix + clienteExistenteEntity.Guid;
        _memoryCache.Remove(cacheKey);
        await _redisDatabase.KeyDeleteAsync(cacheKey);

        _logger.LogInformation($"Datos del cliente eliminados de la base de datos");
        return "Datos del cliente eliminados de la base de datos";
    }
    
    /// <summary>
    /// Borra los datos de un cliente
    /// </summary>
    /// <param name="entityCliente">Cliente a borrar</param>
    /// <returns>Devuelve a un cliente con todos sus datos borrados excepto el id y el guid y sus fechas de creacion y actualizacion</returns>
    private async Task<ClienteEntity> DeleteData(ClienteEntity entityCliente)
    {
        
        entityCliente.Dni = entityCliente.Nombre = entityCliente.Apellidos = entityCliente.Email = entityCliente.Telefono = string.Empty;
        entityCliente.Direccion = new Direccion
        {
            Calle = string.Empty, Numero = string.Empty, CodigoPostal = string.Empty, Piso = string.Empty, Letra = string.Empty
        };
        if (entityCliente.FotoPerfil != null && entityCliente.FotoPerfil != "https://example.com/fotoPerfil.jpg")
        {
            await _fileStorageService.DeleteFileAsync(entityCliente.FotoPerfil);
        }
        if (entityCliente.FotoDni != null && entityCliente.FotoDni != "https://example.com/fotoDni.jpg")
        {
            await _ftpService.DeleteFileAsync(entityCliente.FotoDni);
        }
        entityCliente.FotoPerfil = entityCliente.FotoDni = string.Empty;
        entityCliente.IsDeleted = true;
        entityCliente.UpdatedAt = DateTime.UtcNow;
        return entityCliente;
    }
    
    /// <summary>
    /// Valida si el DNI introducido ya existe en la base de datos
    /// </summary>
    /// <param name="dni"></param>
    /// <exception cref="ClienteExistsException">En caso de que exista en otro cliente</exception>
    private void ValidateDniExistente(string dni)
    {
        _logger.LogInformation("Validando Dni");
        if (_context.Clientes.Any(c => c.Dni.ToLower() == dni.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el DNI: {dni}");
            throw new ClienteExistsException($"Ya existe un cliente con el DNI: {dni}");
        }
    }
    
    /// <summary>
    /// Valida si el email introducido ya existe en la base de datos
    /// </summary>
    /// <param name="email"></param>
    /// <exception cref="ClienteExistsException">En caso de que exista en otro cliente</exception>
    private void ValidateEmailExistente(string email)
    {
        _logger.LogInformation("Validando email");
        if(_context.Clientes.Any(c => c.Email.ToLower() == email.ToLower()))
        {
            _logger.LogInformation($"Ya existe un cliente con el email: {email}");
            throw new ClienteExistsException($"Ya existe un cliente con el email: {email}");
        }
    }
    
    /// <summary>
    /// Valida si el telefono introducido ya existe en la base de datos
    /// </summary>
    /// <param name="telefono"></param>
    /// <exception cref="ClienteExistsException">En caso de que exista en otro cliente </exception>
    private void ValidateTelefonoExistente(string telefono)
    {
        _logger.LogInformation("Validando teléfono");
        if(_context.Clientes.Any(c => c.Telefono == telefono))
        {
            _logger.LogInformation($"Ya existe un cliente con el teléfono: {telefono}");
            throw new ClienteExistsException($"Ya existe un cliente con el teléfono: {telefono}");
        }
    }
    
    /// <summary>
    /// Actualiza la foto de perfil de un cliente
    /// </summary>
    /// <remarks>Se elimina la imagen anterior que tenia en el perfil si no era la predeterminada, existen muchos casos en los que no se permite
    /// el almacenamiento del fichero entre ellos que no sea una imagen, estos se controlan en el servicio de imagenes</remarks>
    /// <param name="userAuth">Usuario autenticado con rol de Cliente</param>
    /// <param name="fotoPerfil">Imagen nueva para el perfil del cliente</param>
    /// <returns>Los datos del cliente con la nueva imagen actualizada</returns>
    /// <exception cref="FileStorageException">En caso de que exista algun error al intentar almacenar la imagen</exception>
    public async Task<ClienteResponse?> UpdateFotoPerfil(User.Models.User userAuth, IFormFile fotoPerfil)
    {
        _logger.LogInformation($"Actualizando foto de perfil del cliente autenticado");

        var clienteEntityExistente = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);
        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente autenticado no encontrado");
            return null;
        }

        var fotoAnterior = clienteEntityExistente.FotoPerfil;

        string nuevaFoto;
        try
        {
            nuevaFoto = await _fileStorageService.SaveFileAsync(fotoPerfil);
        }
        catch (FileStorageException ex)
        {
            _logger.LogError($"Error al guardar la nueva foto de perfil: {ex.Message}");
            throw new FileStorageException($"Error al guardar la foto: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(fotoAnterior) && fotoAnterior != "https://example.com/fotoPerfil.jpg")
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(fotoAnterior);
            }
            catch (FileStorageNotFoundException)
            {
                _logger.LogInformation($"Archivo no encontrado, omitiendo la eliminación: {fotoAnterior}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al intentar eliminar la foto anterior: {ex.Message}");
            }
        }

        clienteEntityExistente.FotoPerfil = nuevaFoto;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();

        return clienteEntityExistente.ToResponseFromEntity();
    }

    /// <summary>
    /// Guarda la foto del DNI de un cliente
    /// </summary>
    /// <param name="userAuth">Usuario autenticado con rol de Cliente</param>
    /// <param name="fotoDni">Fichero con la imagen de dni a actualizar</param>
    /// <returns>Devuelve los datos del cliente </returns>
    /// <exception cref="InvalidOperationException">En caso de que ocurra algun error al intentar almacenarlo en el FTP</exception>
    public async Task<ClienteResponse?> UpdateFotoDni(User.Models.User userAuth, IFormFile fotoDni)
    {
        _logger.LogInformation($"Actualizando foto del DNI del cliente autenticado");

        var clienteEntityExistente = await _context.Clientes.Include(c => c.User)
            .FirstOrDefaultAsync(c => c.User.Guid == userAuth.Guid);

        if (clienteEntityExistente == null)
        {
            _logger.LogInformation($"Cliente autenticado no encontrado");
            return null;
        }
        
        _logger.LogInformation($"Cliente con guid {clienteEntityExistente.Guid} encontrado");

        if (!string.IsNullOrEmpty(clienteEntityExistente.FotoDni) &&
            clienteEntityExistente.FotoDni != "https://example.com/fotoDni.jpg")
        {
            try
            {
                await _ftpService.DeleteFileAsync(clienteEntityExistente.FotoDni);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al eliminar la foto anterior del FTP: {ex.Message}");
            }
        }
        
        string fileExtension = Path.GetExtension(fotoDni.FileName);
        string fileName = $"{clienteEntityExistente.Dni}{fileExtension}";
        string uploadPath = $"data/{fileName}";

        _logger.LogInformation($"Ruta final del archivo para subir: {uploadPath}");

        try
        {
            using (var stream = fotoDni.OpenReadStream())
            {
                await _ftpService.UploadFileAsync(stream, uploadPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al subir la nueva foto al FTP: {ex.Message}");
            throw new InvalidOperationException("Error al subir la nueva foto al servidor FTP.", ex);
        }
        
        clienteEntityExistente.FotoDni = uploadPath;
        clienteEntityExistente.UpdatedAt = DateTime.UtcNow;

        _context.Clientes.Update(clienteEntityExistente);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Foto del DNI del cliente con guid {clienteEntityExistente.Guid} actualizada correctamente.");
        return clienteEntityExistente.ToResponseFromEntity();
    }
    /// <summary>
    /// Descarga la foto de perfil de un cliente
    /// </summary>
    /// <param name="guid">Identificador del cliente</param>
    /// <returns>Devuelve un Stream con la imagen de la foto de perfil</returns> 
    public async Task<Stream> GetFotoDniAsync(string guid)
    {
        _logger.LogInformation($"Buscando foto DNI del cliente con guid: {guid}");

        var clienteEntityExistente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Guid == guid);

        if (clienteEntityExistente == null || string.IsNullOrEmpty(clienteEntityExistente.FotoDni))
            return null;
        
        _logger.LogInformation($"Descargando foto DNI del cliente con guid: {guid}");

        try
        {
            var localTempFile = Path.GetTempFileName();
            await _ftpService.DownloadFileAsync(clienteEntityExistente.FotoDni, localTempFile);

            return new FileStream(localTempFile, FileMode.Open, FileAccess.Read, 
                FileShare.Read, 4096, FileOptions.DeleteOnClose);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al descargar foto DNI: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Metodo que devuelve todos los clientes en la base de datos, los mapea a modelos y los devuelve
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Cliente>> GetAllForStorage()
    {
        _logger.LogInformation("Buscando todos los clientes en la base de datos");
        var clientes = await _context.Clientes.ToListAsync();
        return clientes.Select(c => ClienteMapper.ToModelFromEntity(c));
    }

    /// <summary>
    /// Metodo que devuelve un cliente a partir de un guid
    /// </summary>
    /// <param name="guid">Identificador</param>
    /// <returns>El cliente con el guid asociado</returns>
    public async Task<Models.Cliente?> GetClienteModelByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cliente con guid: {guid}");
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Guid == guid);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con guid: {guid}");
            return clienteEntity.ToModelFromEntity();
        }
        _logger.LogInformation($"Cliente no encontrado con guid: {guid}");
        return null;
    }

    /// <summary>
    /// Devuelve un modelo Cliente a partir de un id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Models.Cliente?> GetClienteModelById(long id)
    {
        _logger.LogInformation($"Buscando Cliente con id: {id}");
        var clienteEntity = await _context.Clientes.Include(c => c.User).FirstOrDefaultAsync(t => t.Id == id);
        if (clienteEntity != null)
        {
            _logger.LogInformation($"Cliente encontrado con id: {id}");
            return clienteEntity.ToModelFromEntity();  
        }

        _logger.LogInformation($"Cliente no encontrado con id: {id}");
        return null;
    }
}
