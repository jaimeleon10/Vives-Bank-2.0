using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Vives_Bank_Net.Rest.Producto.Cuenta.Database;
using Vives_Bank_Net.Rest.Producto.Cuenta.Dto;
using Vives_Bank_Net.Rest.Producto.Cuenta.Exceptions;
using Vives_Bank_Net.Rest.Producto.Cuenta.Mappers;
using Vives_Banks_Net.Rest.Cliente;
using VivesBankProject.Utils.Pagination;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Services;

public class CuentaService : ICuentaService
{
    
    private readonly CuentaDbContext _context;
    private readonly ILogger<CuentaService> _logger;

    public async Task<PageResponse<CuentaResponse>> GetAll(
        BigInteger? saldoMax,
        BigInteger? saldoMin,
        String? tipoCuenta,
        PageRequest pageRequest)
    {
        
        int pageNumber = pageRequest.PageNumber >= 0 ? pageRequest.PageNumber : 0;
        int pageSize = pageRequest.PageSize > 0 ? pageRequest.PageSize : 10;

        var query = _context.Cuentas.AsQueryable();

        if (saldoMax.HasValue)
        {
            query = query.Where(c => c.Saldo <= saldoMax.Value);
        }

        if (saldoMin.HasValue)
        {
            query = query.Where(c => c.Saldo >= saldoMin.Value);
        }
        //TODO
        /*
        if (!string.IsNullOrEmpty(tipoCuenta))
        {
            query = query.Where(c => c.Producto.Nombre.ToString().Contains(tipoCuenta));
        }
        */
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
        //TODO
        //Verificar que el cliente existe
        var query = _context.Cuentas.AsQueryable().Where(c => c.Cliente.Guid == guid); 

        var content = await query.ToListAsync();

        var cuentaResponses = content.Select(c => c.ToCuentaResponse()).ToList();

        return cuentaResponses;
        
    }

    public async Task<CuentaResponse> getByGuid(string guid)
    {
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);

        if (cuenta == null)
        {
            throw new CuentaNoEncontradaException($"Cuenta con Guid {guid} no encontrada.");
        }

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }

    public async Task<CuentaResponse> getByIban(string iban)
    {
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Iban == iban);

        if (cuenta == null)
        {
            throw new CuentaNoEncontradaException($"Cuenta con IBAN {iban} no encontrada.");
        }
        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }

    public async Task<CuentaResponse> save(CuentaRequest cuentaRequest)
    {
        //TODO
        //Comprobar que el tipo de cuenta existe y pasarselo
        //Pasarle el clienteid
        var cuenta = crearCuenta(0,0);
        
        var cuentaEntity = cuenta.ToCuentaEntity();

        await _context.Cuentas.AddAsync(cuentaEntity);

        await _context.SaveChangesAsync();

        var cuentaResponse = cuentaEntity.ToCuentaResponse();
        
        return cuentaResponse;
        
    }

    private Cuenta crearCuenta(long idProducto, long idCliente)
    {
     
        return new Cuenta
        {
            ProductoId = idProducto,
            ClienteId = idCliente
        };

    }

    public async Task<CuentaResponse> update(string guid, CuentaUpdateRequest cuentaRequest)
    {
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  

        if (cuenta == null)
        {
            //TODO
            //Cambiar por excepcion de cliente
            throw new SaldoInsuficiente("La cuenta con el GUID proporcionado no existe.");
        }


        if (BigInteger.TryParse(cuentaRequest.Dinero, out var saldo))
        {
            if (saldo <= 0)
            {
                throw new SaldoInsuficiente("El saldo debe ser mayor a 0.");
            }
            else
            {
             cuenta.Saldo = saldo;   
            }
            
        }
        else
        {
            throw new ArgumentException("El saldo proporcionado no es válido.");
        }

        await _context.SaveChangesAsync();

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }


    public async Task<CuentaResponse> delete(string guid)
    {
        var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Guid == guid);  

        if (cuenta == null)
        {
            //TODO
            //Cambiar por excepcion de cliente
            throw new SaldoInsuficiente("La cuenta con el GUID proporcionado no existe.");
        }

        cuenta.IsDeleted = true;

        await _context.SaveChangesAsync();

        var cuentaResponse = cuenta.ToCuentaResponse();

        return cuentaResponse;
    }


}