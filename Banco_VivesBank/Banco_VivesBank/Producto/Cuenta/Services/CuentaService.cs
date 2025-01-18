using System.Numerics;
using Banco_VivesBank.Database;
using Banco_VivesBank.Producto.Base.Exceptions;
using Banco_VivesBank.Producto.Base.Services;
using Banco_VivesBank.Producto.Cuenta.Dto;
using Banco_VivesBank.Producto.Cuenta.Exceptions;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Producto.Cuenta.Services;

public class CuentaService : ICuentaService
{
    
    private readonly GeneralDbContext _context;
    private readonly IBaseService _baseService;
    private readonly ILogger<CuentaService> _logger;

    public CuentaService(GeneralDbContext context, ILogger<CuentaService> logger, IBaseService baseService)
    {
        _context = context;
        _logger = logger;
        _baseService = baseService;
    }

    public async Task<PageResponse<CuentaResponse>> GetAll(
        BigInteger? saldoMax,
        BigInteger? saldoMin,
        String? tipoCuenta,
        PageRequest pageRequest)
    {
        _logger.LogInformation("Buscando todos las Cuentas en la base de datos");
        int pageNumber = pageRequest.PageNumber >= 0 ? pageRequest.PageNumber : 0;
        int pageSize = pageRequest.PageSize > 0 ? pageRequest.PageSize : 10;

        var query = _context.Cuentas.AsQueryable();

        if (saldoMax.HasValue)
        {
            _logger.LogInformation($"Filtrando por Saldo Maximo: {saldoMax}");
            query = query.Where(c => c.Saldo <= saldoMax.Value);
        }

        if (saldoMin.HasValue)
        {
            _logger.LogInformation($"Filtrando por Saldo Minimo: {saldoMax}");
            query = query.Where(c => c.Saldo >= saldoMin.Value);
        }
        
        if (!string.IsNullOrEmpty(tipoCuenta))
        {
            _logger.LogInformation($"Filtrando por Tipo de cuenta: {tipoCuenta}");
            query = query.Where(c => c.Producto.Nombre.ToString().Contains(tipoCuenta));
        }
        
        if (!string.IsNullOrEmpty(pageRequest.SortBy))
        {
            query = pageRequest.Direction.ToUpper() == "ASC"
                ? query.OrderBy(e => EF.Property<object>(e, pageRequest.SortBy))
                : query.OrderByDescending(e => EF.Property<object>(e, pageRequest.SortBy));
        }

        var totalElements = await query.CountAsync();
        
        var content = await query
            .Skip(pageNumber * pageSize)  
            .Take(pageSize)               
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double) totalElements / pageSize);

        var pageResponse = new PageResponse<CuentaResponse>
        {
            Content = content.Select(c => c.ToCuentaResponse()).ToList(),
            TotalPages = totalPages,
            TotalElements = totalElements,
            PageSize = pageSize,
            PageNumber = pageNumber,
            Empty = !content.Any(),
            First = pageNumber == 0,
            Last = pageNumber == totalPages - 1,
            SortBy = pageRequest.SortBy,
            Direction = pageRequest.Direction
        };

        return pageResponse;
    }

    public async Task<List<CuentaResponse>> getByClientGuid(string guid)
    {
        _logger.LogInformation($"Buscando todos las Cuentas del cliente {guid} en la base de datos");
        //TODO
        //Verificar que el cliente existe
        var query = _context.Cuentas.AsQueryable().Where(c => c.Cliente.Guid == guid); 

        var content = await query.ToListAsync();

        var cuentaResponses = content.Select(c => c.ToCuentaResponse()).ToList();

        return cuentaResponses;
        
    }

    public async Task<CuentaResponse> getByGuid(string guid)
    {
        _logger.LogInformation($"Buscando Cuenta: {guid} en la base de datos");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);

        if (cuenta == null)
        {
            _logger.LogError($"Cuenta: {guid}  no encontrada en la base de datos");
            throw new CuentaNoEncontradaException($"Cuenta con Guid {guid} no encontrada.");
        }

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }

    public async Task<CuentaResponse> getByIban(string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban} en la base de datos");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuenta == null)
        {
            _logger.LogError($"Cuenta con IBAN: {iban}  no encontrada en la base de datos");
            throw new CuentaNoEncontradaException($"Cuenta con IBAN {iban} no encontrada.");
        }
        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }
    
    public async Task<CuentaResponse> getMeByIban(string guid,string iban)
    {
        _logger.LogInformation($"Buscando Cuenta por IBAN: {iban} en la base de datos");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuenta == null)
        {
            _logger.LogError($"Cuenta con IBAN: {iban}  no encontrada en la base de datos");
            throw new CuentaNoEncontradaException($"Cuenta con IBAN {iban} no encontrada.");
        }

        if (cuenta.Cliente.Guid != guid)
        {
            _logger.LogError($"Cuenta con IBAN: {iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {iban}  no le pertenece");
            
        }
        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }

    public async Task<CuentaResponse> save(string guid,CuentaRequest cuentaRequest)
    {
        _logger.LogInformation($"Creando cuenta nueva");
        var tipoCuenta = _baseService.GetByTipoAsync(cuentaRequest.TipoCuenta);
        
        if (tipoCuenta == null)
        {
            _logger.LogError($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            throw new BaseNotExistException($"El tipo de Cuenta {cuentaRequest.TipoCuenta}  no existe en nuestro catalogo");
            
        }
        //TODO
        //buscar el cliente y pasarle el id
        //Pasarle el clienteid
        var cuenta =crearCuenta(tipoCuenta.Id,0);
        
        var cuentaEntity = cuenta.ToCuentaEntity();

        await _context.Cuentas.AddAsync(cuentaEntity);

        await _context.SaveChangesAsync();

        var cuentaResponse = cuentaEntity.ToCuentaResponse();
        
        return cuentaResponse;
        
    }

    private Banco_VivesBank.Producto.Cuenta.Models.Cuenta crearCuenta(long idProducto, long idCliente)
    {
     
        return new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            ProductoId = idProducto,
            ClienteId = idCliente
        };

    }

    public async Task<CuentaResponse> update(string guidClient,string guid, CuentaUpdateRequest cuentaRequest)
    {
        _logger.LogInformation($"Actualizando cuenta {guid}");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  

        if (cuenta == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            //TODO
            //Cambiar por excepcion de cliente
            throw new SaldoInsuficienteException($"La cuenta con el GUID {guid} no existe.");
        }
        
        if (cuenta.Cliente.Guid != guidClient)
        {
            _logger.LogError($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            
        }


        if (BigInteger.TryParse(cuentaRequest.Dinero, out var saldo))
        {
            if (saldo <= 0)
            {
                _logger.LogError($"Saldo insuficiente para actualizar.");
                throw new SaldoInsuficienteException("El saldo debe ser mayor a 0.");
            }
            else
            {
             cuenta.Saldo = saldo;   
            }
            
        }
        else
        {
            _logger.LogError($"El saldo proporcionado no es válido.");
            throw new SaldoInvalidoException("El saldo proporcionado no es válido.");
        }

        await _context.SaveChangesAsync();

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }


    public async Task<CuentaResponse> delete(string guidClient,string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  

        if (cuenta == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            //TODO
            //Cambiar por excepcion de cliente
            throw new SaldoInsuficienteException($"La cuenta con el GUID {guid} no existe.");
        }
        
        if (cuenta.Cliente.Guid != guidClient)
        {
            _logger.LogError($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            
        }

        cuenta.IsDeleted = true;

        await _context.SaveChangesAsync();

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }
    
    public async Task<CuentaResponse> deleteAdmin(string guidClient,string guid)
    {
        _logger.LogInformation($"Eliminando cuenta {guid}");
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  

        if (cuenta == null)
        {
            _logger.LogError($"La cuenta con el GUID {guid} no existe.");
            //TODO
            //Cambiar por excepcion de cliente
            throw new SaldoInsuficienteException($"La cuenta con el GUID {guid} no existe.");
        }
        
        if (cuenta.Cliente.Guid != guidClient)
        {
            _logger.LogError($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            throw new CuentaNoPertenecienteAlUsuarioException($"Cuenta con IBAN: {cuenta.Iban}  no le pertenece");
            
        }

        cuenta.IsDeleted = true;

        await _context.SaveChangesAsync();

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }


}