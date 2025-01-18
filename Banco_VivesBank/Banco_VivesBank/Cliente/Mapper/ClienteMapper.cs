using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;

namespace Banco_VivesBank.Cliente.Mapper;

public class ClienteMapper
{
    public static Models.Cliente ToModelFromRequest(ClienteRequest userRequest, User.Models.User user)
    {
        return new Models.Cliente
        {
            Dni = userRequest.Dni,
            Nombre = userRequest.Nombre,
            Apellidos = userRequest.Apellidos,
            Direccion = new Direccion
            {
                Calle = userRequest.Calle,
                Numero = userRequest.Numero,
                CodigoPostal = userRequest.CodigoPostal,
                Piso = userRequest.Piso,
                Letra = userRequest.Letra,
            },
            Email = userRequest.Email,
            Telefono = userRequest.Telefono,
            User = user,
            IsDeleted = userRequest.IsDeleted
        };
    }
    
    public static ClienteEntity ToEntityFromModel(Models.Cliente cliente)
    {
        return new ClienteEntity
        {
            Id = cliente.Id,
            Guid = cliente.Guid,
            Dni = cliente.Dni,
            Nombre = cliente.Nombre,
            Apellidos = cliente.Apellidos,
            Direccion = cliente.Direccion,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FotoPerfil = cliente.FotoPerfil,
            FotoDni = cliente.FotoDni,
            UserId = cliente.User.Id,
            CreatedAt = cliente.CreatedAt,
            UpdatedAt = cliente.UpdatedAt,
            IsDeleted = cliente.IsDeleted
        };
    }

    public static ClienteResponse ToResponseFromModel(Models.Cliente cliente)
    {
        return new ClienteResponse
        {
            Guid = cliente.Guid,
            Dni = cliente.Dni,
            Nombre = cliente.Nombre,
            Apellidos = cliente.Apellidos,
            Direccion = cliente.Direccion,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FotoPerfil = cliente.FotoPerfil,
            FotoDni = cliente.FotoDni,
            UserId = cliente.User.Guid,
            CreatedAt = cliente.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = cliente.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = cliente.IsDeleted
        };
    }

    public static ClienteResponse ToResponseFromEntity(ClienteEntity clienteEntity, User.Models.User user)
    {
        return new ClienteResponse
        {
            Guid = clienteEntity.Guid,
            Dni = clienteEntity.Dni,
            Nombre = clienteEntity.Nombre,
            Apellidos = clienteEntity.Apellidos,
            Direccion = clienteEntity.Direccion,
            Email = clienteEntity.Email,
            Telefono = clienteEntity.Telefono,
            FotoPerfil = clienteEntity.FotoPerfil,
            FotoDni = clienteEntity.FotoDni,
            UserId = user.Guid,
            CreatedAt = clienteEntity.CreatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            UpdatedAt = clienteEntity.UpdatedAt.ToString("dd/MM/yyyy - HH:mm:ss"),
            IsDeleted = clienteEntity.IsDeleted
        };
    }
}